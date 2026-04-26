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

public class ProcessCalculatePositionPerformanceHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IReturnCalculator> _returnCalculator = new();
    private readonly ProcessCalculatePositionPerformanceHandler _handler;

    private static readonly DateOnly Today = new(2024, 6, 15);
    private static readonly DateOnly Yesterday = Today.PreviousTradingDay();

    public ProcessCalculatePositionPerformanceHandlerTests()
    {
        _context = TestContextFactory.Create();
        _returnCalculator.Setup(r => r.Calculate(
                It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), It.IsAny<PerformanceScope>(), It.IsAny<bool>()))
            .Returns(0.05m);

        var timeProvider = new Microsoft.Extensions.Time.Testing.FakeTimeProvider();
        var ukNoon = new DateTime(Today.Year, Today.Month, Today.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, Hoard.Core.DateOnlyHelper.UkTimeZone);
        timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));

        _handler = new ProcessCalculatePositionPerformanceHandler(
            NullLogger<ProcessCalculatePositionPerformanceHandler>.Instance,
            _context, _bus.Object, _returnCalculator.Object, timeProvider);
    }

    private async Task<(Portfolio portfolio, Account account, Instrument instrument, Position position)> SetupOpenPosition(
        DateOnly? openDate = null)
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        var position = new Position
        {
            InstrumentId = instrument.Id,
            PortfolioId = portfolio.Id,
            OpenDate = openDate ?? Today.AddYears(-2)
        };
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        return (portfolio, account, instrument, position);
    }

    private async Task<(Portfolio portfolio, Account account, Instrument instrument, Position position)> SetupClosedPosition(
        DateOnly? openDate = null, DateOnly? closeDate = null)
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        var position = new Position
        {
            InstrumentId = instrument.Id,
            PortfolioId = portfolio.Id,
            OpenDate = openDate ?? Today.AddYears(-2),
            CloseDate = closeDate ?? Today.AddMonths(-1)
        };
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        return (portfolio, account, instrument, position);
    }

    private async Task AddHoldings(int instrumentId, int accountId,
        params (DateOnly date, decimal units, decimal value)[] entries)
    {
        foreach (var (date, units, value) in entries)
        {
            var holding = new Holding
            {
                InstrumentId = instrumentId,
                AccountId = accountId,
                AsOfDate = date,
                Units = units,
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
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_CalculatesIncomeFromIncomeTransactions()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.AddRange(
            new Transaction
            {
                AccountId = account.Id, InstrumentId = instrument.Id,
                TransactionTypeId = TransactionType.IncomeDividend,
                Date = Today.AddMonths(-3), Value = 100m
            },
            new Transaction
            {
                AccountId = account.Id, InstrumentId = instrument.Id,
                TransactionTypeId = TransactionType.IncomeInterest,
                Date = Today.AddMonths(-1), Value = 25m
            });
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 100, 5000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Equal(125m, perf.Income);
    }

    [Fact]
    public async Task HandleAsync_CalculatesCostBasisAndRealisedGain()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddMonths(-3), price: 120m, units: -20, value: 2400m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 30, 3900m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // CostBasis: buy 50 units @ total cost 5000, sell 20 @ avg cost 100 = cost removed 2000, remaining cost = 3000
        Assert.Equal(3000m, perf.CostBasis);
        // RealisedGain: sale proceeds 2400 - cost removed 2000 = 400
        Assert.Equal(400m, perf.RealisedGain);
    }

    [Fact]
    public async Task HandleAsync_CalculatesUnitsAndValueFromTodaysHoldings()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 75, value: -7500m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 75, 9000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Equal(75m, perf.Units);
        Assert.Equal(9000m, perf.Value);
    }

    [Fact]
    public async Task HandleAsync_CalculatesUnrealisedGain()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 50, 7500m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // Unrealised = Value - CostBasis = 7500 - 5000 = 2500
        Assert.Equal(2500m, perf.UnrealisedGain);
    }

    [Fact]
    public async Task HandleAsync_ClosedPosition_SetsZeroUnitsValueAndUnrealisedGain()
    {
        var (_, account, instrument, _) = await SetupClosedPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-2), price: 100m, units: 50, value: -5000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddMonths(-1), price: 120m, units: -50, value: 6000m));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Equal(0m, perf.Units);
        Assert.Equal(0m, perf.Value);
        Assert.Equal(0m, perf.UnrealisedGain);
    }

    [Fact]
    public async Task HandleAsync_ClosedPosition_AllReturnsAndValueChangesAreNull()
    {
        var (_, account, instrument, _) = await SetupClosedPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-2), price: 100m, units: 50, value: -5000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddMonths(-1), price: 120m, units: -50, value: 6000m));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Null(perf.Return1D);
        Assert.Null(perf.Return1W);
        Assert.Null(perf.Return1M);
        Assert.Null(perf.Return3M);
        Assert.Null(perf.Return6M);
        Assert.Null(perf.Return1Y);
        Assert.Null(perf.Return3Y);
        Assert.Null(perf.Return5Y);
        Assert.Null(perf.Return10Y);
        Assert.Null(perf.ReturnYtd);
        Assert.Null(perf.ValueChange1D);
        Assert.Null(perf.ValueChange1Y);
    }

    [Fact]
    public async Task HandleAsync_NewPosition_ReturnsNullForPeriodsBeforeOpenDate()
    {
        var (_, account, instrument, _) = await SetupOpenPosition(openDate: Today.AddDays(-5));

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddDays(-5), price: 100m, units: 10, value: -1000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 10, 1050m),
            (Yesterday, 10, 1040m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // Position opened 5 days ago — 1D return should be non-null (yesterday is within range)
        Assert.NotNull(perf.Return1D);
        // 1W: startDate = today-7, openDate(today-5) > startDate+1(today-6) → null
        Assert.Null(perf.Return1W);
        Assert.Null(perf.Return1M);
        Assert.Null(perf.Return3M);
        Assert.Null(perf.Return6M);
        Assert.Null(perf.Return1Y);
    }

    [Fact]
    public async Task HandleAsync_CalculatesAbsoluteValueChange1D()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 50, 5000m),
            (Today, 50, 5200m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // No transactions in the 1-day period, so change = end - start = 5200 - 5000 = 200
        Assert.Equal(200m, perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_AbsoluteValueChange_AdjustsForContributions()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m),
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today, price: 110m, units: 10, value: -1100m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 50, 5000m),
            (Today, 60, 6200m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // end=6200 + withdrawals=0 - contributions=1100 - start=5000 = 100
        Assert.Equal(100m, perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_AbsoluteValueChange_AdjustsForWithdrawals()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 100, value: -10000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today, price: 110m, units: -20, value: 2200m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 100, 10000m),
            (Today, 80, 8800m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // end=8800 + withdrawals(sell)=2200 - contributions=0 - start=10000 = 1000
        Assert.Equal(1000m, perf.ValueChange1D);
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectScopeToReturnCalculator()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 50, 5000m),
            (Today, 50, 5200m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        _returnCalculator.Verify(r => r.Calculate(
            It.IsAny<decimal>(), It.IsAny<decimal>(),
            It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
            It.IsAny<IList<Transaction>>(), PerformanceScope.Position, It.IsAny<bool>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_CalculatesAllTimeReturn()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 50, 5200m));

        // Override the catch-all with specific setups for the all-time calls,
        // then verify they were actually matched
        _returnCalculator.Setup(r => r.Calculate(
                0m, 5200m,
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), PerformanceScope.Position, false))
            .Returns(0.04m)
            .Verifiable();

        _returnCalculator.Setup(r => r.Calculate(
                0m, 5200m,
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), PerformanceScope.Position, true))
            .Returns(0.02m)
            .Verifiable();

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        _returnCalculator.Verify();

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Equal(0.04m, perf.ReturnAllTime);
        Assert.Equal(0.02m, perf.AnnualisedReturn);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingPerformance()
    {
        var (_, account, instrument, position) = await SetupOpenPosition();

        _context.PositionPerformances.Add(new PositionPerformance
        {
            PositionId = position.Id, Value = 1000m, UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Today, 50, 7000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perfs = await _context.PositionPerformances.ToListAsync();
        Assert.Single(perfs);
        Assert.Equal(7000m, perfs[0].Value);
    }

    [Fact]
    public async Task HandleAsync_MultiplePositionsForSameInstrument_IsolatesValues()
    {
        var portfolio1 = TestData.Portfolio(id: 1, name: "Portfolio 1");
        var portfolio2 = TestData.Portfolio(id: 2, name: "Portfolio 2");
        var account1 = TestData.Account(id: 1, name: "Account 1");
        var account2 = TestData.Account(id: 2, name: "Account 2");
        portfolio1.Accounts.Add(account1);
        portfolio2.Accounts.Add(account2);
        _context.Portfolios.AddRange(portfolio1, portfolio2);

        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        var position1 = new Position { InstrumentId = instrument.Id, PortfolioId = portfolio1.Id, OpenDate = Today.AddYears(-2) };
        var position2 = new Position { InstrumentId = instrument.Id, PortfolioId = portfolio2.Id, OpenDate = Today.AddYears(-1) };
        _context.Positions.AddRange(position1, position2);
        await _context.SaveChangesAsync();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account1.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-2), price: 100m, units: 30, value: -3000m),
            TestData.BuyTransaction(accountId: account2.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 110m, units: 20, value: -2200m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account1.Id, (Today, 30, 3600m));
        await AddHoldings(instrument.Id, account2.Id, (Today, 20, 2500m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perfs = await _context.PositionPerformances.ToListAsync();
        Assert.Equal(2, perfs.Count);

        var perf1 = perfs.Single(p => p.PositionId == position1.Id);
        var perf2 = perfs.Single(p => p.PositionId == position2.Id);
        Assert.Equal(3600m, perf1.Value);
        Assert.Equal(30m, perf1.Units);
        Assert.Equal(2500m, perf2.Value);
        Assert.Equal(20m, perf2.Units);
    }

    [Fact]
    public async Task HandleAsync_NoHoldingsForToday_SetsZeroUnitsAndValue()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        // Holdings exist but not for today
        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 50, 5000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        Assert.Equal(0m, perf.Units);
        Assert.Equal(0m, perf.Value);
    }

    [Fact]
    public async Task HandleAsync_PublishesPositionPerformanceCalculatedEvent()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.Add(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 10, value: -1000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id, (Today, 10, 1100m));

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            runId, instrument.Id, PipelineMode.CloseOfDay));

        _bus.Verify(b => b.Publish(It.Is<PositionPerformanceCalculatedEvent>(e =>
            e.PerformanceRunId == runId &&
            e.InstrumentId == instrument.Id &&
            e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_FiltersTransactionsByPositionDateRange()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        var openDate = Today.AddMonths(-6);
        var position = new Position
        {
            InstrumentId = instrument.Id,
            PortfolioId = portfolio.Id,
            OpenDate = openDate
        };
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        // Transaction before position open date — should be excluded from cost basis
        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 50m, units: 100, value: -5000m),
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: openDate, price: 100m, units: 50, value: -5000m));
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id, (Today, 50, 6000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // Only the second buy (within position date range) should count
        Assert.Equal(5000m, perf.CostBasis);
        Assert.Equal(0m, perf.RealisedGain);
    }

    [Fact]
    public async Task HandleAsync_ClosedPosition_StillCalculatesCostBasisAndRealisedGain()
    {
        var (_, account, instrument, _) = await SetupClosedPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-2), price: 100m, units: 50, value: -5000m),
            TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddMonths(-1), price: 130m, units: -50, value: 6500m));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // Cost removed = avg cost 100 * 50 = 5000, realised = 6500 - 5000 = 1500
        Assert.Equal(0m, perf.CostBasis);
        Assert.Equal(1500m, perf.RealisedGain);
    }

    [Fact]
    public async Task HandleAsync_DividendIncomeIncludedInAbsoluteValueChangeAsWithdrawal()
    {
        var (_, account, instrument, _) = await SetupOpenPosition();

        _context.Transactions.AddRange(
            TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
                date: Today.AddYears(-1), price: 100m, units: 50, value: -5000m),
            new Transaction
            {
                AccountId = account.Id, InstrumentId = instrument.Id,
                TransactionTypeId = TransactionType.IncomeDividend,
                Date = Today, Value = 150m
            });
        await _context.SaveChangesAsync();

        await AddHoldings(instrument.Id, account.Id,
            (Yesterday, 50, 5000m),
            (Today, 50, 5000m));

        await _handler.HandleAsync(new ProcessCalculatePositionPerformanceCommand(
            Guid.NewGuid(), instrument.Id, PipelineMode.CloseOfDay));

        var perf = await _context.PositionPerformances.SingleAsync();
        // end=5000 + withdrawals(dividend=150) - contributions=0 - start=5000 = 150
        Assert.Equal(150m, perf.ValueChange1D);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
