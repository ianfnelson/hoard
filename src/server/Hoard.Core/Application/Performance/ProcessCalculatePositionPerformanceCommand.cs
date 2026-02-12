using Hoard.Core.Data;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePositionPerformanceCommand(Guid PerformanceRunId, int InstrumentId, PipelineMode PipelineMode) : ICommand;

public class ProcessCalculatePositionPerformanceHandler(
    ILogger<ProcessCalculatePositionPerformanceHandler> logger, 
    HoardContext context, IBus bus, IReturnCalculator returnCalculator)
: ICommandHandler<ProcessCalculatePositionPerformanceCommand>
{
    public async Task HandleAsync(ProcessCalculatePositionPerformanceCommand command, CancellationToken ct = default)
    {
        var (performanceRunId, instrumentId, pipelineMode) = command;
        
        logger.LogInformation("Calculating Position Performance for Instrument {InstrumentId}", instrumentId);
        
        var positions = await GetPositions(instrumentId, ct);
        var transactions = await GetTransactions(instrumentId, ct);
        var holdings = await GetHoldings(instrumentId, ct);

        foreach (var position in positions)
        {
            await UpsertPerformance(position, transactions, holdings, ct);
        }

        await bus.Publish(new PositionPerformanceCalculatedEvent(performanceRunId, instrumentId, pipelineMode));
    }

    private sealed record PositionContext(
        Position Position,
        int[] AccountIds,
        Dictionary<DateOnly, Holding[]> Holdings,
        List<Transaction> Transactions,
        DateOnly Today,
        DateOnly PreviousTradingDay
        );
    
    private async Task UpsertPerformance(
        Position position, 
        List<Transaction> transactions, 
        Dictionary<DateOnly, Holding[]> holdings,
        CancellationToken ct)
    {
        var perf = await LoadOrCreate(position, ct);

        var contextData = BuildPositionContext(position, transactions, holdings);
        
        CalculateStaticMetrics(perf, contextData);
        CalculateReturns(perf, contextData);
            
        perf.UpdatedUtc = DateTime.UtcNow;
            
        await context.SaveChangesAsync(ct);
    }

    private void CalculateReturns(PositionPerformance perf, PositionContext ctx)
    {
        var (position, a, h, t, today, previousDay) = ctx;

        perf.Return1D = CalculatePeriodReturn(ctx, previousDay, today);
        perf.Return1W = CalculatePeriodReturn(ctx, today.AddDays(-7), today);
        perf.Return1M = CalculatePeriodReturn(ctx, today.AddMonths(-1), today);
        perf.Return3M = CalculatePeriodReturn(ctx, today.AddMonths(-3), today);
        perf.Return6M = CalculatePeriodReturn(ctx, today.AddMonths(-6), today);
        perf.Return1Y = CalculatePeriodReturn(ctx, today.AddYears(-1), today);
        perf.Return3Y = CalculatePeriodReturn(ctx, today.AddYears(-3), today);
        perf.Return5Y = CalculatePeriodReturn(ctx, today.AddYears(-5), today);
        perf.Return10Y = CalculatePeriodReturn(ctx, today.AddYears(-10), today);
        perf.ReturnYtd = CalculatePeriodReturn(ctx, new DateOnly(today.Year-1,12,31), today);

        // Calculate absolute value change for 1-year period
        perf.ValueChange1Y = CalculateAbsoluteValueChange(ctx, today.AddYears(-1), today);
        
        var startDate = t.Select(x => x.Date).Min().AddDays(-1);
        var endDate = ctx.Position.CloseDate ?? ctx.Today;
        
        perf.ReturnAllTime = returnCalculator.Calculate(decimal.Zero, perf.Value, startDate, endDate, t, PerformanceScope.Position);
        perf.AnnualisedReturn = returnCalculator.Calculate(decimal.Zero, perf.Value, startDate, endDate, t, PerformanceScope.Position, annualised: true);
    }

    private async Task<PositionPerformance> LoadOrCreate(Position position, CancellationToken ct)
    {
        var perf = await context.PositionPerformances
            .FirstOrDefaultAsync(x => x.PositionId == position.Id, ct);

        if (perf is null)
        {
            perf = new PositionPerformance { PositionId = position.Id };
            context.PositionPerformances.Add(perf);
        }

        return perf;
    }

    private static PositionContext BuildPositionContext(
        Position position,
        List<Transaction> allTransactions,
        Dictionary<DateOnly, Holding[]> holdings)
    {
        var today = DateOnlyHelper.TodayLocal();
        var previousDay = today.PreviousTradingDay();
        var accountIds = position.Portfolio.Accounts.Select(a => a.Id).ToArray();

        var transactionsForPosition = allTransactions
            .Where(t => accountIds.Contains(t.AccountId))
            .Where(t => t.Date >= position.OpenDate &&
                        (!position.CloseDate.HasValue || t.Date <= position.CloseDate))
            .ToList();

        return new PositionContext(position, accountIds, holdings, transactionsForPosition, today, previousDay);
    }

    private static void CalculateStaticMetrics(PositionPerformance perf, PositionContext ctx)
    {
        var (position, a, h, t, today, previousDay) = ctx;
        
        perf.Income = t
            .Where(x => TransactionTypeSets.Income.Contains(x.TransactionTypeId))
            .Sum(x => x.Value);
        
        var (costBasis,realisedGain) = CostBasisCalculator.Calculate(t);

        perf.CostBasis = costBasis;
        perf.RealisedGain = realisedGain;
        
        if (position.CloseDate.HasValue)
        {
            perf.Units = 0;
            perf.Value = 0;
            perf.PreviousValue = 0;
            perf.ValueChange = 0;
            perf.UnrealisedGain = 0;
            return;
        }
        
        perf.Units = GetUnitsForDate(today, h, a) ?? decimal.Zero;
        perf.Value = GetValueForDate(today, h, a) ?? decimal.Zero;
        
        perf.PreviousValue = GetValueForDate(previousDay, h, a) ?? decimal.Zero;
        perf.ValueChange = perf.Value - perf.PreviousValue;
        
        perf.UnrealisedGain = perf.Value - perf.CostBasis;
    }

    private decimal? CalculatePeriodReturn(PositionContext ctx, DateOnly startDate, DateOnly endDate)
    {
        var (position, accountIds, holdings, transactions, _, _) = ctx;

        // No returns for closed positions
        if (position.CloseDate.HasValue)
        {
            return null;
        }

        // No returns for positions that were not open the day after start date
        if (position.OpenDate > startDate.AddDays(1))
        {
            return null;
        }

        var startValue = GetValueForDate(startDate, holdings, accountIds) ?? 0M;
        var endValue = GetValueForDate(endDate, holdings, accountIds) ?? 0M;

        var periodTransactions = transactions.Where(x => x.Date > startDate && x.Date <= endDate).ToList();

        return returnCalculator.Calculate(startValue, endValue, startDate, endDate, periodTransactions, PerformanceScope.Position);
    }

    private static decimal? CalculateAbsoluteValueChange(PositionContext ctx, DateOnly startDate, DateOnly endDate)
    {
        var (position, accountIds, holdings, transactions, _, _) = ctx;

        // No value change for closed positions
        if (position.CloseDate.HasValue)
        {
            return null;
        }

        // No value change for positions that were not open the day after start date
        if (position.OpenDate > startDate.AddDays(1))
        {
            return null;
        }

        var startValue = GetValueForDate(startDate, holdings, accountIds);
        var endValue = GetValueForDate(endDate, holdings, accountIds);

        if (!startValue.HasValue || !endValue.HasValue)
        {
            return null;
        }

        // Get buy/sell/income/corporate action transactions in the period
        var periodTransactions = transactions.Where(x => x.Date > startDate && x.Date <= endDate).ToList();

        var periodWithdrawals = periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Sell ||
                        t.TransactionTypeId == TransactionType.IncomeDividend ||
                        t.TransactionTypeId == TransactionType.IncomeLoyaltyBonus ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: > decimal.Zero })
            .Sum(t => t.Value);

        var periodContributions = periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Buy ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: < decimal.Zero })
            .Sum(t => -t.Value);

        // Absolute change = end value + withdrawals - contributions - start value
        return endValue.Value + periodWithdrawals - periodContributions - startValue.Value;
    }

    private static decimal? GetUnitsForDate(DateOnly date, Dictionary<DateOnly, Holding[]> holdings, int[] accountIds)
    {
        if (!holdings.TryGetValue(date, out var holdingsForDate))
        {
            return null;
        }

        var holdingsForAccounts = holdingsForDate.Where(x => accountIds.Contains(x.AccountId)).ToList();
        return holdingsForAccounts.Count == 0 ? null : holdingsForAccounts.Sum(x => x.Units);
    }
    
    private static decimal? GetValueForDate(DateOnly date, Dictionary<DateOnly, Holding[]> holdings, int[] accountIds)
    {
        if (!holdings.TryGetValue(date, out var holdingsForDate))
        {
            return null;
        }
        var holdingsForAccounts = holdingsForDate.Where(x => accountIds.Contains(x.AccountId)).ToList();
        return holdingsForAccounts.Count == 0 ? null : holdingsForAccounts.Sum(x => x.Valuation?.Value ?? 0);
    }

    private async Task<List<Position>> GetPositions(int instrumentId, CancellationToken ct)
    {
        var positions = await context.Positions
            .Where(p => p.InstrumentId == instrumentId)
            .Include(p => p.Portfolio)
            .Include(p => p.Portfolio.Accounts)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return positions;
    }

    private async Task<List<Transaction>> GetTransactions(int instrumentId, CancellationToken ct)
    {
        var transactions = await context.Transactions
            .Where(t => t.InstrumentId == instrumentId)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return transactions;
    }

    private async Task<Dictionary<DateOnly, Holding[]>> GetHoldings(int instrumentId, CancellationToken ct)
    {
        var holdings = await context.Holdings
            .Include(h => h.Valuation)
            .Where(h => h.InstrumentId == instrumentId)
            .AsNoTracking()
            .ToListAsync(ct);

        return holdings.GroupBy(x => x.AsOfDate)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}