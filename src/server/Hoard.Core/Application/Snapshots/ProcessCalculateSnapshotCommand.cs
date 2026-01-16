using Hoard.Core.Data;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Snapshots;

public record ProcessCalculateSnapshotCommand(Guid SnapshotsRunId, PipelineMode PipelineMode, int PortfolioId, int Year)
    : ICommand;

public class ProcessCalculateSnapshotHandler( IBus bus, ILogger<ProcessCalculateSnapshotHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculateSnapshotCommand>
{
    public async Task HandleAsync(ProcessCalculateSnapshotCommand command, CancellationToken ct = default)
    {
        var (snapshotsRunId, pipelineMode, portfolioId, year) = command;
        
        logger.LogInformation("Calculating Portfolio Snapshot for Portfolio {PortfolioId}, Year {Year}", portfolioId, year);

        var portfolio = await GetPortfolio(portfolioId, ct);
        var transactions = await GetTransactions(portfolio, year, ct);
        var valuations = await GetValuations(portfolio, year, ct);

        await UpsertSnapshot(portfolio, year, transactions, valuations, ct);
        
        await bus.Publish(new SnapshotCalculatedEvent(snapshotsRunId, pipelineMode, year, portfolioId));
    }

    private async Task UpsertSnapshot(Portfolio portfolio, int year, List<Transaction> transactions, Dictionary<DateOnly, PortfolioValuation> valuations, CancellationToken ct)
    {
        var snapshot = await LoadOrCreate(portfolio, year, ct);
        
        CalculateTransactionMetrics(snapshot, transactions);
        CalculateValuationMetrics(snapshot, valuations);
        CalculateDerivedMetrics(snapshot);
        
        snapshot.UpdatedUtc = DateTime.UtcNow;
        
        await context.SaveChangesAsync(ct);
    }

    private static void CalculateTransactionMetrics(PortfolioSnapshot snapshot, List<Transaction> t)
    {
        snapshot.CountTrades =
            t.Count(x => x.TransactionTypeId is TransactionType.Buy or TransactionType.Sell);

        snapshot.TotalBuys = t
            .Where(x => x.TransactionTypeId == TransactionType.Buy ||
                        x is { TransactionTypeId: TransactionType.CorporateAction, Value: < decimal.Zero })
            .Sum(x => -x.Value);
        
        snapshot.TotalSells = t
            .Where(x => x.TransactionTypeId == TransactionType.Sell ||
                        x is { TransactionTypeId: TransactionType.CorporateAction, Value: > decimal.Zero })
            .Sum(x => x.Value);
        
        snapshot.TotalIncomeInterest = GetTransactionTotal(t, TransactionType.IncomeInterest);
        snapshot.TotalIncomeLoyaltyBonus = GetTransactionTotal(t, TransactionType.IncomeLoyaltyBonus);
        snapshot.TotalIncomePromotion = GetTransactionTotal(t, TransactionType.IncomePromotion);
        snapshot.TotalIncomeDividends = GetTransactionTotal(t, TransactionType.IncomeDividend);

        snapshot.TotalFees = -GetTransactionTotal(t, TransactionType.Fee);
        
        snapshot.TotalDealingCharge = t.Sum(x => x.DealingCharge ?? decimal.Zero);
        snapshot.TotalStampDuty = t.Sum(x => x.StampDuty ?? decimal.Zero);
        snapshot.TotalPtmLevy = t.Sum(x => x.PtmLevy ?? decimal.Zero);
        snapshot.TotalFxCharge = t.Sum(x => x.FxCharge ?? decimal.Zero);
        
        snapshot.TotalDepositEmployer = GetTransactionTotal(t, TransactionType.DepositEmployer);
        snapshot.TotalDepositIncomeTaxReclaim = GetTransactionTotal(t, TransactionType.DepositIncomeTaxReclaim);
        snapshot.TotalDepositTransferIn = GetTransactionTotal(t, TransactionType.DepositTransfer);
        snapshot.TotalDepositPersonal = GetTransactionTotal(t, TransactionType.DepositPersonal);
        
        snapshot.TotalWithdrawals = GetTransactionTotal(t, TransactionType.Withdrawal);
    }

    private static decimal GetTransactionTotal(List<Transaction> transactions, int transactionTypeId)
    {
        return transactions
            .Where(x => x.TransactionTypeId == transactionTypeId)
            .Sum(x => x.Value);
    }

    private static void CalculateValuationMetrics(PortfolioSnapshot snapshot, Dictionary<DateOnly, PortfolioValuation> valuations)
    {
        var earliestDate = valuations.Keys.Min();
        var latestDate = valuations.Keys.Max();
        
        snapshot.StartValue = earliestDate.Year == latestDate.Year ? decimal.Zero : valuations[earliestDate].Value;
        snapshot.EndValue = valuations[latestDate].Value;
        
        snapshot.ValueChange = snapshot.EndValue - snapshot.StartValue;
        
        snapshot.AverageValue = valuations
            .Where(kvp => kvp.Key != earliestDate)
            .Average(kvp => kvp.Value.Value);
    }

    private static void CalculateDerivedMetrics(PortfolioSnapshot snapshot)
    {
        snapshot.Return = SimpleReturnCalculator.Calculate(snapshot.StartValue, snapshot.EndValue,
            snapshot.TotalWithdrawals, snapshot.TotalDepositEmployer + snapshot.TotalDepositPersonal + snapshot.TotalDepositIncomeTaxReclaim + snapshot.TotalDepositTransferIn);
        
        snapshot.Churn = 100.0M * Math.Max(snapshot.TotalBuys, snapshot.TotalSells) / snapshot.AverageValue;
    }

    private async Task<PortfolioSnapshot> LoadOrCreate(Portfolio portfolio, int year, CancellationToken ct)
    {
        var snapshot = await context.PortfolioSnapshots
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolio.Id && x.Year == year, ct);

        if (snapshot is null)
        {
            snapshot = new PortfolioSnapshot { PortfolioId = portfolio.Id, Year = year };
            context.PortfolioSnapshots.Add(snapshot);
        }
        
        return snapshot;
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
    
    private async Task<List<Transaction>> GetTransactions(Portfolio portfolio, int year, CancellationToken ct)
    {
        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(x => portfolio.Accounts.Select(x1 => x1.Id).Contains(x.AccountId))
            .Where(x => x.Date.Year == year)
            .ToListAsync(ct);
        
        return transactions;
    }
    
    private async Task<Dictionary<DateOnly, PortfolioValuation>> GetValuations(Portfolio portfolio, int year, CancellationToken ct)
    {
        var startDate = new DateOnly(year-1, 12, 31);
        var endDate = new DateOnly(year, 12, 31);
        
        var valuations = await context.PortfolioValuations
            .Where(x => x.PortfolioId == portfolio.Id)
            .Where(x => x.AsOfDate >= startDate && x.AsOfDate <= endDate)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return valuations.GroupBy(x => x.AsOfDate)
            .ToDictionary(g => g.Key, g => g.First());
    }
}