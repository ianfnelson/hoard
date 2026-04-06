using Hoard.Core.Application.Performance;
using Hoard.Core.Data;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Performance;

public class ProcessCalculatePortfolioPerformanceHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IReturnCalculator> _returnCalculator = new();
    private readonly ProcessCalculatePortfolioPerformanceHandler _handler;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Now);
    private static readonly DateOnly Yesterday = Today.PreviousTradingDay();

    public ProcessCalculatePortfolioPerformanceHandlerTests()
    {
        _context = TestContextFactory.Create();
        _returnCalculator.Setup(r => r.Calculate(
                It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), It.IsAny<PerformanceScope>(), It.IsAny<bool>()))
            .Returns(0.05m);

        _handler = new ProcessCalculatePortfolioPerformanceHandler(
            NullLogger<ProcessCalculatePortfolioPerformanceHandler>.Instance,
            _context, _bus.Object, _returnCalculator.Object);
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

    private async Task AddPortfolioValuations(int portfolioId,
        params (DateOnly date, decimal value)[] entries)
    {
        foreach (var (date, value) in entries)
        {
            _context.PortfolioValuations.Add(new PortfolioValuation
            {
                PortfolioId = portfolioId, AsOfDate = date, Value = value, UpdatedUtc = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task AddPositionPerformance(int portfolioId, int instrumentId,
        decimal value, decimal unrealisedGain, decimal realisedGain)
    {
        var position = new Position
        {
            InstrumentId = instrumentId,
            PortfolioId = portfolioId,
            OpenDate = Today.AddYears(-2)
        };
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        _context.PositionPerformances.Add(new PositionPerformance
        {
            PositionId = position.Id,
            Value = value,
            UnrealisedGain = unrealisedGain,
            RealisedGain = realisedGain,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    private async Task AddCashHolding(int accountId, DateOnly date, decimal value)
    {
        var holding = new Holding
        {
            InstrumentId = Instrument.Cash,
            AccountId = accountId,
            AsOfDate = date,
            Units = value,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();

        _context.HoldingValuations.Add(new HoldingValuation
        {
            HoldingId = holding.Id,
            Value = value,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_CalculatesValueFromPortfolioValuation()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        var instrument = TestData.Instrument(id: 2);
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 15000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(15000m, perf.Value);
    }

    [Fact]
    public async Task HandleAsync_AggregatesUnrealisedAndRealisedGainsFromPositions()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        var instrument1 = TestData.Instrument(id: 2, name: "Stock A", ticker: "AAA");
        var instrument2 = TestData.Instrument(id: 3, name: "Stock B", ticker: "BBB");
        _context.Instruments.AddRange(instrument1, instrument2);
        await _context.SaveChangesAsync();

        await AddPositionPerformance(portfolio.Id, instrument1.Id, 5000m, 500m, 200m);
        await AddPositionPerformance(portfolio.Id, instrument2.Id, 3000m, 300m, 100m);

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument1.Id,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 8000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(800m, perf.UnrealisedGain);
        Assert.Equal(300m, perf.RealisedGain);
    }

    [Fact]
    public async Task HandleAsync_CalculatesIncomeFromAllTransactions()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.AddRange(
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.IncomeDividend,
                Date = Today.AddMonths(-3), Value = 200m
            },
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.IncomeInterest,
                Date = Today.AddMonths(-1), Value = 50m
            },
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.IncomeLoyaltyBonus,
                Date = Today.AddMonths(-2), Value = 10m
            },
            // Non-income transaction — should not be counted
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 10000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(260m, perf.Income);
    }

    [Fact]
    public async Task HandleAsync_CalculatesCashValue()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Instruments.Add(new Instrument
        {
            Id = Instrument.Cash, Name = "Cash", Ticker = "CASH", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1
        });
        await _context.SaveChangesAsync();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddCashHolding(account.Id, Today, 2500m);
        await AddPortfolioValuations(portfolio.Id, (Today, 12500m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(2500m, perf.CashValue);
    }

    [Fact]
    public async Task HandleAsync_CalculatesAbsoluteValueChange1D()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id,
            (Yesterday, 10000m),
            (Today, 10500m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        // No transactions in the period, so change = end - start = 500
        Assert.Equal(500m, perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_AbsoluteValueChange_AdjustsForDepositsAndWithdrawals()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)),
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.DepositPersonal,
                Date = Today, Value = 1000m
            },
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.Withdrawal,
                Date = Today, Value = -500m
            });
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id,
            (Yesterday, 10000m),
            (Today, 10800m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        // end=10800 + withdrawals(500) - contributions(1000) - start=10000 = 300
        Assert.Equal(300m, perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_AbsoluteValueChange_NullWhenNoStartValuation()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        // Only today's valuation, no yesterday
        await AddPortfolioValuations(portfolio.Id, (Today, 10000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Null(perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_CalculatesYieldFromIncomeAndAverageValue()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.AddRange(
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.IncomeDividend,
                Date = Today.AddMonths(-6), Value = 500m
            },
            // Earliest transaction required so Min() over transaction dates doesn't throw
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-2)));
        await _context.SaveChangesAsync();

        // Valuations across the trailing year (all within the (oneYearAgo, today] window)
        var oneYearAgo = Today.AddYears(-1);
        await AddPortfolioValuations(portfolio.Id,
            (oneYearAgo.AddMonths(3), 9000m),
            (oneYearAgo.AddMonths(6), 10000m),
            (oneYearAgo.AddMonths(9), 11000m),
            (Today, 12000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        var expectedAverage = (9000m + 10000m + 11000m + 12000m) / 4m;
        Assert.Equal(100.0m * 500m / expectedAverage, perf.Yield);
    }

    [Fact]
    public async Task HandleAsync_YieldIsZeroWhenNoValuationsInPeriod()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        // No valuations within the 1-year window (only one outside)
        await AddPortfolioValuations(portfolio.Id,
            (Today.AddYears(-2), 5000m),
            (Today, 10000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        // Only Today valuation is within the 1-year range (oneYearAgo, today]
        // Average = 10000, income = 0 so yield = 0
        Assert.Equal(0m, perf.Yield);
    }

    [Fact]
    public async Task HandleAsync_PassesPortfolioScopeToReturnCalculator()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id,
            (Yesterday, 10000m),
            (Today, 10500m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        _returnCalculator.Verify(r => r.Calculate(
            It.IsAny<decimal>(), It.IsAny<decimal>(),
            It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
            It.IsAny<IList<Transaction>>(), PerformanceScope.Portfolio, It.IsAny<bool>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_CalculatesAllTimeAndAnnualisedReturn()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-2)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 15000m));

        _returnCalculator.Setup(r => r.Calculate(
                0m, 15000m,
                It.IsAny<DateOnly>(), Today,
                It.IsAny<IList<Transaction>>(), PerformanceScope.Portfolio, false))
            .Returns(0.50m)
            .Verifiable();

        _returnCalculator.Setup(r => r.Calculate(
                0m, 15000m,
                It.IsAny<DateOnly>(), Today,
                It.IsAny<IList<Transaction>>(), PerformanceScope.Portfolio, true))
            .Returns(0.15m)
            .Verifiable();

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        _returnCalculator.Verify();

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(0.50m, perf.ReturnAllTime);
        Assert.Equal(0.15m, perf.AnnualisedReturn);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingPerformance()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.PortfolioPerformances.Add(new PortfolioPerformance
        {
            PortfolioId = portfolio.Id, Value = 5000m, UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 20000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perfs = await _context.PortfolioPerformances.ToListAsync();
        Assert.Single(perfs);
        Assert.Equal(20000m, perfs[0].Value);
    }

    [Fact]
    public async Task HandleAsync_PublishesPortfolioPerformanceCalculatedEvent()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 10000m));

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            runId, portfolio.Id, PipelineMode.CloseOfDay));

        _bus.Verify(b => b.Publish(It.Is<PortfolioPerformanceCalculatedEvent>(e =>
            e.PerformanceRunId == runId &&
            e.PortfolioId == portfolio.Id &&
            e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PeriodReturn_IncludesTransactionsOnEndDate()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        // Deposit dated on Today (the end date for 1D return)
        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)),
            new Transaction
            {
                AccountId = account.Id,
                TransactionTypeId = TransactionType.DepositPersonal,
                Date = Today, Value = 1000m
            });
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id,
            (Yesterday, 10000m),
            (Today, 11000m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        // The 1D return calculator should receive a list containing the deposit on Today
        _returnCalculator.Verify(r => r.Calculate(
            10000m, 11000m,
            Yesterday, Today,
            It.Is<IList<Transaction>>(t => t.Any(x => x.Date == Today && x.Value == 1000m)),
            PerformanceScope.Portfolio, It.IsAny<bool>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_PortfolioNotFound_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
                Guid.NewGuid(), 999, PipelineMode.CloseOfDay)));
    }

    [Fact]
    public async Task HandleAsync_NoPositionPerformances_SetsZeroGains()
    {
        var (portfolio, account) = await SetupPortfolioWithAccount();

        // Earliest transaction required so Min() over transaction dates doesn't throw
        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: null,
                date: Today.AddYears(-1)));
        await _context.SaveChangesAsync();

        await AddPortfolioValuations(portfolio.Id, (Today, 0m));

        await _handler.HandleAsync(new ProcessCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), portfolio.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PortfolioPerformances.SingleAsync();
        Assert.Equal(0m, perf.Value);
        Assert.Equal(0m, perf.UnrealisedGain);
        Assert.Equal(0m, perf.RealisedGain);
        Assert.Equal(0m, perf.Income);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
