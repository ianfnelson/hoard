using Hoard.Core.Application.Valuations;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Valuations;

public class ProcessCalculateHoldingValuationsHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly ProcessCalculateHoldingsValuationHandler _handler;

    public ProcessCalculateHoldingValuationsHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessCalculateHoldingsValuationHandler(
            _bus.Object,
            NullLogger<ProcessCalculateHoldingsValuationHandler>.Instance,
            _context);
    }

    [Fact]
    public async Task HandleAsync_NoHoldings_DoesNotPublishChangeEvent()
    {
        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            runId, PipelineMode.CloseOfDay, 999, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<HoldingValuationsChangedEvent>(), null), Times.Never);
        _bus.Verify(b => b.Publish(It.IsAny<HoldingValuationsCalculatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CashHolding_ValuationEqualsUnits()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        var holding = new Holding
        {
            AccountId = account.Id,
            InstrumentId = Instrument.Cash,
            Instrument = new Instrument { Id = Instrument.Cash, Name = "Cash", Ticker = "CASH", CurrencyId = Currency.Gbp, InstrumentTypeId = 1, AssetSubclassId = 1 },
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 5000m,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            runId, PipelineMode.CloseOfDay, Instrument.Cash, new DateOnly(2024, 1, 15)));

        var valuation = await _context.HoldingValuations.SingleAsync();
        Assert.Equal(5000m, valuation.Value);
    }

    [Fact]
    public async Task HandleAsync_GbpInstrument_ValuationUsesPrice()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Prices.Add(TestData.Price(instrumentId: 10, asOfDate: new DateOnly(2024, 1, 15), close: 150m));

        var holding = new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 20m,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();

        // Detach so the handler can re-load with includes


        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        var valuation = await _context.HoldingValuations.SingleAsync();
        Assert.Equal(3000m, valuation.Value); // 20 * 150
    }

    [Fact]
    public async Task HandleAsync_PublishesChangedEvent_WhenValuationIsNew()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Prices.Add(TestData.Price(instrumentId: 10, asOfDate: new DateOnly(2024, 1, 15), close: 100m));

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 10m,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();


        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            runId, PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.Is<HoldingValuationsChangedEvent>(e =>
            e.ValuationsRunId == runId && e.InstrumentId == 10), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PublishesCalculatedEvent_Always()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Prices.Add(TestData.Price(instrumentId: 10, asOfDate: new DateOnly(2024, 1, 15), close: 100m));

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 10m,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();


        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            runId, PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.Is<HoldingValuationsCalculatedEvent>(e =>
            e.ValuationsRunId == runId), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DoesNotPublishChangedEvent_WhenValueUnchanged()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Prices.Add(TestData.Price(instrumentId: 10, asOfDate: new DateOnly(2024, 1, 15), close: 100m));

        var holding = new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 10m,
            UpdatedUtc = DateTime.UtcNow
        };
        _context.Holdings.Add(holding);
        await _context.SaveChangesAsync();

        // Pre-create valuation with the expected value
        _context.HoldingValuations.Add(new HoldingValuation
        {
            HoldingId = holding.Id,
            Value = 1000m, // 10 * 100
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();


        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<HoldingValuationsChangedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoPriceAvailable_ValuationIsZero()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 10m,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();


        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        var valuation = await _context.HoldingValuations.SingleAsync();
        Assert.Equal(0m, valuation.Value);
    }

    [Fact]
    public async Task HandleAsync_GbxCurrency_DividesByHundred()
    {
        var account = TestData.Account();
        var instrument = new Instrument { Id = 10, Name = "Test", Ticker = "TST", CurrencyId = Currency.Gbx, InstrumentTypeId = 1, AssetSubclassId = 1 };
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Prices.Add(TestData.Price(instrumentId: 10, asOfDate: new DateOnly(2024, 1, 15), close: 500m));

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id,
            InstrumentId = 10,
            AsOfDate = new DateOnly(2024, 1, 15),
            Units = 20m,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();


        await _handler.HandleAsync(new ProcessCalculateHoldingValuationsCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 10, new DateOnly(2024, 1, 15)));

        var valuation = await _context.HoldingValuations.SingleAsync();
        Assert.Equal(100m, valuation.Value); // 20 * 500 / 100
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
