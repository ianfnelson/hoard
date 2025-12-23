using Hoard.Core.Data;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using PortfolioValuation = Hoard.Core.Domain.Entities.PortfolioValuation;

namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePortfolioPerformanceCommand(Guid PerformanceRunId, int PortfolioId, PipelineMode PipelineMode) : ICommand;

public class ProcessCalculatePortfolioPerformanceHandler(ILogger<ProcessCalculatePortfolioPerformanceHandler> logger, HoardContext context,
    IBus bus) : ICommandHandler<ProcessCalculatePortfolioPerformanceCommand>
{
    public async Task HandleAsync(ProcessCalculatePortfolioPerformanceCommand command, CancellationToken ct = default)
    {
        var (performanceRunId, portfolioId, pipelineMode) = command;
        
        logger.LogInformation("Calculating Portfolio Performance for Portfolio {PortfolioId}", portfolioId);
        
        var portfolio = await GetPortfolio(portfolioId, ct);
        var transactions = await GetTransactions(portfolio, ct);
        var positionPerformances = await GetPositionPerformances(portfolio, ct);
        var valuations = await GetValuations(portfolio, ct);
 
        await UpsertPerformance(portfolio, transactions, positionPerformances, valuations, ct);
        
        await bus.Publish(new PortfolioPerformanceCalculatedEvent(performanceRunId, portfolioId, pipelineMode));
    }

    private async Task<decimal> GetCash(Portfolio portfolio, DateOnly asOfDate, CancellationToken ct)
    {
        var cash = await context.HoldingValuations
            .AsNoTracking()
            .Where(x => portfolio.Accounts.Select(x1 => x1.Id).Contains(x.Holding.AccountId))
            .Where(x => x.Holding.InstrumentId == Instrument.Cash)
            .Where(x => x.Holding.AsOfDate == asOfDate)
            .SumAsync(x => x.Value, ct);

        return cash;
    }

    private async Task<List<Transaction>> GetTransactions(Portfolio portfolio, CancellationToken ct)
    {
        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(x => portfolio.Accounts.Select(x1 => x1.Id).Contains(x.AccountId))
            .Where(x => new []{TransactionCategory.Deposit, TransactionCategory.Withdrawal, TransactionCategory.CorporateAction}.Contains(x.CategoryId))
            .ToListAsync(ct);
        
        return transactions;
    }

    private async Task<List<PositionPerformance>> GetPositionPerformances(Portfolio portfolio, CancellationToken ct)
    {
        var positionPerformances = await context.PositionPerformances
            .Where (x => x.Position.PortfolioId == portfolio.Id)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return positionPerformances;
    }

    private async Task<Dictionary<DateOnly, PortfolioValuation>> GetValuations(Portfolio portfolio, CancellationToken ct)
    {
        var valuations = await context.PortfolioValuations
            .Where(x => x.PortfolioId == portfolio.Id)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return valuations.GroupBy(x => x.AsOfDate)
            .ToDictionary(g => g.Key, g => g.First());
    }
    
    private async Task<Portfolio> GetPortfolio(int portfolioId, CancellationToken ct)
    {
        var portfolio = await context.Portfolios
            .Include(x => x.Accounts)
            .Where(p => p.Id == portfolioId)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return portfolio ?? throw new InvalidOperationException($"No Portfolio found with Id {portfolioId}");
    }

    private async Task UpsertPerformance(
        Portfolio portfolio,
        List<Transaction> transactions,
        List<PositionPerformance> positionPerformances,
        Dictionary<DateOnly, PortfolioValuation> valuations,
        CancellationToken ct)
    {
        var perf = await LoadOrCreate(portfolio, ct);

        var contextData = BuildPortfolioContext(portfolio, transactions, valuations, positionPerformances);

        await CalculateStaticMetrics(perf, contextData, ct);
        CalculateReturns(perf, contextData);
        
        perf.UpdatedUtc = DateTime.UtcNow;
        
        logger.LogInformation(
            "Portfolio {PortfolioId,5} | Value {Value,12:N2} | Δ {Change,10:N2} | 1Y {Return1Y,8:N2}%",
            portfolio.Id,
            perf.Value,
            perf.ValueChange,
            perf.Return1Y
        );
       
        await context.SaveChangesAsync(ct);
    }

    private async Task CalculateStaticMetrics(PortfolioPerformance perf, PortfolioContext ctx, CancellationToken ct)
    {
        var (portfolio, _, valuations, positionPerformances, today, previousDay) = ctx;

        perf.Value = GetValueForDate(today, valuations) ?? decimal.Zero;
        perf.PreviousValue = GetValueForDate(previousDay, valuations) ?? decimal.Zero;
        perf.ValueChange = perf.Value - perf.PreviousValue;
        
        perf.UnrealisedGain = positionPerformances.Sum(x => x.UnrealisedGain);
        perf.RealisedGain = positionPerformances.Sum(x => x.RealisedGain);
        perf.Income = positionPerformances.Sum(x => x.Income);

        perf.CashValue = await GetCash(portfolio, today, ct);
    }

    private static void CalculateReturns(PortfolioPerformance perf, PortfolioContext ctx)
    {
        var (_, transactions, _, _, today, previousDay) = ctx;
        
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
        
        var startDate = transactions.Select(x => x.Date).Min();
        
        perf.ReturnAllTime = SimpleReturnCalculator.CalculateForPortfolio(decimal.Zero, perf.Value, transactions);
        perf.AnnualisedReturn = AnnualisedReturnCalculator.Calculate(perf.ReturnAllTime, startDate, today);
    }
    
    private static decimal? CalculatePeriodReturn(PortfolioContext ctx, DateOnly startDate, DateOnly endDate)
    {
        var (_, transactions, valuations, _, _, _) = ctx;

        var valueStart = GetValueForDate(startDate, valuations) ?? 0M;
        var valueEnd = GetValueForDate(endDate, valuations) ?? 0M;
        
        var periodTransactions = transactions.Where(x => x.Date > startDate && x.Date < endDate).ToList();
        
        return SimpleReturnCalculator.CalculateForPortfolio(valueStart, valueEnd, periodTransactions);
    }

    private static decimal? GetValueForDate(DateOnly previousDay, Dictionary<DateOnly, PortfolioValuation> valuations)
    {
        if (!valuations.TryGetValue(previousDay, out var valuation))
        {
            return null;
        }
        
        return valuation.Value;
    }

    private static PortfolioContext BuildPortfolioContext(
        Portfolio portfolio,
        List<Transaction> transactions,
        Dictionary<DateOnly, PortfolioValuation> valuations,
        List<PositionPerformance> positionPerformances)
    {
        var today = DateOnlyHelper.TodayLocal();
        var previousDay = today.PreviousTradingDay();
        
        return new PortfolioContext(
            portfolio, transactions, valuations, positionPerformances, today, previousDay);
    }
    
    private sealed record PortfolioContext(
        Portfolio Portfolio,
        List<Transaction> Transactions,
        Dictionary<DateOnly, PortfolioValuation> Valuations,
        List<PositionPerformance> PositionPerformances,
        DateOnly Today,
        DateOnly PreviousTradingDay);
    
    private async Task<PortfolioPerformance> LoadOrCreate(Portfolio portfolio, CancellationToken ct)
    {
        var perf = await context.PortfolioPerformances
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolio.Id, ct);

        if (perf is null)
        {
            perf = new PortfolioPerformance{ PortfolioId =  portfolio.Id };
            context.PortfolioPerformances.Add(perf);
        }

        return perf;
    }
}