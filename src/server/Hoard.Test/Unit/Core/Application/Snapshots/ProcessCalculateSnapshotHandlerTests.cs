using Hoard.Core.Application.Snapshots;
using Hoard.Core.Data;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Snapshots;

public class ProcessCalculateSnapshotHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IReturnCalculator> _returnCalculator = new();
    private readonly ProcessCalculateSnapshotHandler _handler;

    public ProcessCalculateSnapshotHandlerTests()
    {
        _context = TestContextFactory.Create();
        _returnCalculator.Setup(r => r.Calculate(
            It.IsAny<decimal>(), It.IsAny<decimal>(),
            It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
            It.IsAny<IList<Transaction>>(), It.IsAny<PerformanceScope>(), It.IsAny<bool>()))
            .Returns(0.10m);

        _handler = new ProcessCalculateSnapshotHandler(
            _bus.Object,
            NullLogger<ProcessCalculateSnapshotHandler>.Instance,
            _context,
            _returnCalculator.Object);
    }

    private async Task<(Portfolio portfolio, Account account)> SetupPortfolioWithAccount()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return (portfolio, account);
    }

    private async Task AddValuations(int portfolioId, params (DateOnly date, decimal value)[] valuations)
    {
        foreach (var (date, value) in valuations)
        {
            _context.PortfolioValuations.Add(new PortfolioValuation
            {
                PortfolioId = portfolioId, AsOfDate = date, Value = value, UpdatedUtc = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_CalculatesTradeCount()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id, date: new DateOnly(2024, 3, 1)),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id, date: new DateOnly(2024, 6, 1)),
            new Transaction { AccountId = account.Id, TransactionTypeId = TransactionType.IncomeDividend, InstrumentId = instrument.Id, Date = new DateOnly(2024, 9, 1), Value = 50m }
        );
        await _context.SaveChangesAsync();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 6, 30), 11000m),
            (new DateOnly(2024, 12, 31), 12000m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshot = await _context.PortfolioSnapshots.SingleAsync();
        Assert.Equal(2, snapshot.CountTrades); // Buy + Sell, not dividend
    }

    [Fact]
    public async Task HandleAsync_CalculatesBuysAndSells()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id, date: new DateOnly(2024, 3, 1), value: -2000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id, date: new DateOnly(2024, 6, 1), value: 3000m)
        );
        await _context.SaveChangesAsync();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 12, 31), 11000m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshot = await _context.PortfolioSnapshots.SingleAsync();
        Assert.Equal(2000m, snapshot.TotalBuys);
        Assert.Equal(3000m, snapshot.TotalSells);
    }

    [Fact]
    public async Task HandleAsync_CalculatesIncomeTotals()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.AddRange(
            new Transaction { AccountId = account.Id, TransactionTypeId = TransactionType.IncomeDividend, Date = new DateOnly(2024, 3, 1), Value = 100m },
            new Transaction { AccountId = account.Id, TransactionTypeId = TransactionType.IncomeInterest, Date = new DateOnly(2024, 6, 1), Value = 25m },
            new Transaction { AccountId = account.Id, TransactionTypeId = TransactionType.IncomeLoyaltyBonus, Date = new DateOnly(2024, 9, 1), Value = 10m }
        );
        await _context.SaveChangesAsync();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 12, 31), 10135m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshot = await _context.PortfolioSnapshots.SingleAsync();
        Assert.Equal(100m, snapshot.TotalIncomeDividends);
        Assert.Equal(25m, snapshot.TotalIncomeInterest);
        Assert.Equal(10m, snapshot.TotalIncomeLoyaltyBonus);
    }

    [Fact]
    public async Task HandleAsync_CalculatesStartAndEndValues()
    {
        var (portfolio, _) = await SetupPortfolioWithAccount();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 8000m),
            (new DateOnly(2024, 6, 30), 9000m),
            (new DateOnly(2024, 12, 31), 10000m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshot = await _context.PortfolioSnapshots.SingleAsync();
        Assert.Equal(8000m, snapshot.StartValue);
        Assert.Equal(10000m, snapshot.EndValue);
    }

    [Fact]
    public async Task HandleAsync_CalculatesChurn()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id, date: new DateOnly(2024, 3, 1), value: -5000m));
        await _context.SaveChangesAsync();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 12, 31), 15000m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshot = await _context.PortfolioSnapshots.SingleAsync();
        // AverageValue = 15000 (only non-start valuation), Churn = 100 * max(5000,0) / 15000
        Assert.Equal(100.0m * 5000m / 15000m, snapshot.Churn);
    }

    [Fact]
    public async Task HandleAsync_PublishesSnapshotCalculatedEvent()
    {
        var (portfolio, _) = await SetupPortfolioWithAccount();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 12, 31), 10000m));

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            runId, PipelineMode.CloseOfDay, portfolio.Id, 2024));

        _bus.Verify(b => b.Publish(It.Is<SnapshotCalculatedEvent>(e =>
            e.SnapshotsRunId == runId && e.PortfolioId == portfolio.Id && e.Year == 2024), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingSnapshot()
    {
        var (portfolio, _) = await SetupPortfolioWithAccount();

        _context.PortfolioSnapshots.Add(new PortfolioSnapshot
        {
            PortfolioId = portfolio.Id, Year = 2024, EndValue = 5000m, AverageValue = 1m, UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await AddValuations(portfolio.Id,
            (new DateOnly(2023, 12, 31), 10000m),
            (new DateOnly(2024, 12, 31), 12000m));

        await _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, 2024));

        var snapshots = await _context.PortfolioSnapshots.ToListAsync();
        Assert.Single(snapshots);
        Assert.Equal(12000m, snapshots[0].EndValue);
    }

    [Fact]
    public async Task HandleAsync_PortfolioNotFound_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(new ProcessCalculateSnapshotCommand(
                Guid.NewGuid(), PipelineMode.CloseOfDay, 999, 2024)));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
