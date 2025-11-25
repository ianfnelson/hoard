using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Domain.Extensions;
using Hoard.Core.Extensions;
using Hoard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePositionPerformancesCommand(Guid CorrelationId, int InstrumentId, bool IsBackfill = false) : ICommand;

public class ProcessCalculatePositionPerformancesHandler(ILogger<ProcessCalculatePositionPerformancesHandler> logger, HoardContext context)
: ICommandHandler<ProcessCalculatePositionPerformancesCommand>
{
    public async Task HandleAsync(ProcessCalculatePositionPerformancesCommand command, CancellationToken ct = default)
    {
        var (correlationId, instrumentId, isBackfill) = command;
        
        logger.LogInformation("Calculating Position Performances for Instrument {InstrumentId}", instrumentId);
        
        var positions = await GetPositions(instrumentId, ct);
        var transactions = await GetTransactions(instrumentId, ct);
        var holdings = await GetHoldings(instrumentId, ct);

        foreach (var position in positions)
        {
            await UpsertPerformanceCumulative(position, transactions, holdings, ct);
        }
    }

    private async Task UpsertPerformanceCumulative(
        Position position, 
        List<Transaction> transactions, 
        Dictionary<DateOnly, Holding[]> holdings,
        CancellationToken ct)
    {
        var perf = await context.PositionPerformancesCumulative
            .FirstOrDefaultAsync(x => x.PositionId == position.Id, ct);

        if (perf is null)
        {
            perf = new PositionPerformanceCumulative{ PositionId = position.Id};
            context.PositionPerformancesCumulative.Add(perf);
        }

        var today = DateOnlyHelper.TodayLocal();
        var previousDay = PreviousTradingDay(today);
        var accountIds = position.Portfolio.Accounts.Select(x => x.Id).ToArray();
        
        var transactionsForPosition = transactions
            .Where(t => accountIds.Contains(t.AccountId))
            .Where(t => t.Date >= position.OpenDate && (!position.CloseDate.HasValue || t.Date <= position.CloseDate))
            .ToList();
        
        perf.Income = transactionsForPosition
            .Where(x => x.CategoryId == TransactionCategory.Income)
            .Sum(x => x.Value);
        
        var cashflows = transactionsForPosition.ToPositionCashflows();
        var (costBasis,realisedGain) = CostBasisCalculator.Calculate(cashflows);

        perf.CostBasis = costBasis;
        perf.RealisedGain = realisedGain;
        
        if (position.CloseDate.HasValue)
        {
            perf.Units = decimal.Zero;
            perf.Value = decimal.Zero;
            perf.PreviousValue = decimal.Zero;
            perf.ValueChange = decimal.Zero;
            perf.UnrealisedGain = decimal.Zero;
        }
        else
        {
            perf.Units = GetUnitsForDate(today, holdings, accountIds) ?? decimal.Zero;
            perf.Value = GetValueForDate(today, holdings, accountIds) ?? decimal.Zero;
            perf.PreviousValue = GetValueForDate(previousDay, holdings, accountIds) ?? decimal.Zero;
            perf.ValueChange = perf.Value - perf.PreviousValue;
            perf.UnrealisedGain = perf.Value - perf.CostBasis;
        }
        
        perf.Return1D = CalculateReturn(position, holdings, accountIds, transactionsForPosition, previousDay, today);
        perf.Return1W = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddDays(-7), today);
        perf.Return1M = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddMonths(-1), today);
        perf.Return3M = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddMonths(-3), today);
        perf.Return6M = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddMonths(-6), today);
        perf.Return1Y = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddYears(-1), today);
        perf.Return3Y = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddYears(-3), today);
        perf.Return5Y = CalculateReturn(position, holdings, accountIds, transactionsForPosition, today.AddYears(-5), today);
        perf.ReturnYtd = CalculateReturn(position, holdings, accountIds, transactionsForPosition, new DateOnly(today.Year-1,12,31), today);
        perf.ReturnAllTime = CalculateReturn(position, holdings, accountIds, transactionsForPosition, position.OpenDate.AddDays(-1), position.CloseDate ?? today);
        
        perf.AnnualisedReturn = CalculateAnnualisedReturn(perf.ReturnAllTime, position.OpenDate, position.CloseDate ?? today);

        perf.PortfolioWeightPercent = 0;    // TODO
            
        perf.UpdatedUtc = DateTime.UtcNow;
            
        await context.SaveChangesAsync(ct);
    }

    private static DateOnly PreviousTradingDay(DateOnly today)
    {
        return today.DayOfWeek switch
        {
            DayOfWeek.Saturday => today.AddDays(-2),
            DayOfWeek.Sunday => today.AddDays(-3),
            _ => today.AddDays(-1)
        };
    }

    private static decimal? CalculateReturn(
        Position position, 
        Dictionary<DateOnly, Holding[]> holdings, 
        int[] accountIds, 
        List<Transaction> transactions, 
        DateOnly startDate, 
        DateOnly endDate)
    {
        var exposureEnd = position.CloseDate ?? endDate;
        
        var effectiveStart = position.OpenDate >= startDate ? position.OpenDate.AddDays(-1) : startDate;
        var effectiveEnd = exposureEnd < endDate ? exposureEnd : endDate;

        if (effectiveStart >= effectiveEnd)
        {
            return null;
        }

        var valueStart = GetValueForDate(effectiveStart, holdings, accountIds) ?? 0M;
        var valueEnd = GetValueForDate(effectiveEnd, holdings, accountIds) ?? 0M;
        
        var incomeValue = transactions
            .Where(x => x.Date > effectiveStart && x.Date <= effectiveEnd)
            .Where(x => x.CategoryId == TransactionCategory.Income).Sum(x => x.Value);
        
        return ModifiedDietzCalculator.Calculate(
            valueStart, 
            valueEnd, 
            incomeValue, 
            effectiveStart, 
            effectiveEnd, 
            transactions.ToPositionCashflows());
    }

    private static decimal? CalculateAnnualisedReturn(decimal? returnPercentage, DateOnly startDate, DateOnly endDate)
    {
        if (!returnPercentage.HasValue)
        {
            return null;
        }
        
        var growth = 1.0 + (double)returnPercentage.Value / 100.0;
        
        // total wipeout or pathological negative return?
        if (growth <= 0.0)
        {
            return -100m;
        }

        var days = endDate.DayNumber - startDate.DayNumber;
        var years = days / 365.25;

        if (years <= 0.0)
        {
            return null;
        }

        var annualisedGrowth = Math.Pow(growth, 1.0 / years);
        var annualisedReturn = (annualisedGrowth - 1.0) * 100.0;

        const double upperCap = 10000.0;
        const double lowerCap = -100.0;

        if (double.IsNaN(annualisedReturn) || double.IsInfinity(annualisedReturn) || annualisedReturn > upperCap)
        {
            annualisedReturn = upperCap;
        }
        else if (annualisedReturn < lowerCap)
        {
            annualisedReturn = lowerCap;
        }

        return (decimal)annualisedReturn;
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