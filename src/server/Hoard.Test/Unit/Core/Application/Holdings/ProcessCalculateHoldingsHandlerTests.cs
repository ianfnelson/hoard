using Hoard.Core.Application.Holdings;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Holdings;

public class ProcessCalculateHoldingsHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly ProcessCalculateHoldingsHandler _handler;

    public ProcessCalculateHoldingsHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessCalculateHoldingsHandler(
            _bus.Object,
            NullLogger<ProcessCalculateHoldingsHandler>.Instance,
            _context);
    }

    [Fact]
    public async Task HandleAsync_CreatesHoldingsFromBuyTransactions()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 10), units: 10));
        await _context.SaveChangesAsync();

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var holding = await _context.Holdings.SingleOrDefaultAsync(h => h.InstrumentId == instrument.Id);
        Assert.NotNull(holding);
        Assert.Equal(10m, holding.Units);
        Assert.Equal(account.Id, holding.AccountId);
        Assert.Equal(new DateOnly(2024, 1, 15), holding.AsOfDate);
    }

    [Fact]
    public async Task HandleAsync_AggregatesBuysAndSells()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 5), units: 20));
        _context.Transactions.Add(TestData.SellTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 10), units: -5));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var holding = await _context.Holdings.SingleOrDefaultAsync(h =>
            h.InstrumentId == instrument.Id && h.AccountId == account.Id);
        Assert.NotNull(holding);
        Assert.Equal(15m, holding.Units);
    }

    [Fact]
    public async Task HandleAsync_CreatesCashHolding()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 5), units: 10, value: -1000m));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var cashHolding = await _context.Holdings.SingleOrDefaultAsync(h => h.InstrumentId == Instrument.Cash);
        Assert.NotNull(cashHolding);
        Assert.Equal(-1000m, cashHolding.Units);
    }

    [Fact]
    public async Task HandleAsync_IgnoresTransactionsAfterAsOfDate()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 20), units: 10));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var holdings = await _context.Holdings.Where(h => h.InstrumentId == instrument.Id).ToListAsync();
        Assert.Empty(holdings);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingHoldingWhenUnitsChange()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id, InstrumentId = instrument.Id,
            AsOfDate = new DateOnly(2024, 1, 15), Units = 5, UpdatedUtc = DateTime.UtcNow
        });

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 5), units: 10));
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var holding = await _context.Holdings.SingleAsync(h => h.InstrumentId == instrument.Id);
        Assert.Equal(10m, holding.Units);
    }

    [Fact]
    public async Task HandleAsync_RemovesObsoleteHoldings()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id, InstrumentId = instrument.Id,
            AsOfDate = new DateOnly(2024, 1, 15), Units = 10, UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // No transactions exist, so the holding should be removed
        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        var holdings = await _context.Holdings.Where(h => h.InstrumentId == instrument.Id).ToListAsync();
        Assert.Empty(holdings);
    }

    [Fact]
    public async Task HandleAsync_PublishesHoldingChangedEventForChangedInstruments()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        _context.Transactions.Add(TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 5), units: 10, value: -1000m));
        await _context.SaveChangesAsync();

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.Is<HoldingChangedEvent>(e =>
            e.HoldingsRunId == runId &&
            e.InstrumentId == instrument.Id &&
            e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PublishesHoldingsCalculatedEvent()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.Is<HoldingsCalculatedEvent>(e =>
            e.HoldingsRunId == runId &&
            e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DoesNotPublishChangeEventWhenUnitsUnchanged()
    {
        var account = TestData.Account();
        var instrument = TestData.Instrument(id: 10);
        _context.Accounts.Add(account);
        _context.Instruments.Add(instrument);

        var buyTx = TestData.BuyTransaction(accountId: account.Id, instrumentId: instrument.Id,
            date: new DateOnly(2024, 1, 5), units: 10, value: -1000m);

        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id, InstrumentId = instrument.Id,
            AsOfDate = new DateOnly(2024, 1, 15), Units = 10, UpdatedUtc = DateTime.UtcNow
        });
        // Also pre-seed the cash holding that would result from the buy transaction
        _context.Holdings.Add(new Holding
        {
            AccountId = account.Id, InstrumentId = Instrument.Cash,
            AsOfDate = new DateOnly(2024, 1, 15), Units = -1000m, UpdatedUtc = DateTime.UtcNow
        });

        _context.Transactions.Add(buyTx);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<HoldingChangedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExcludesDepositTransactionsFromInstrumentHoldings()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        // Deposit has no instrument
        _context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            InstrumentId = null,
            TransactionTypeId = TransactionType.DepositPersonal,
            Date = new DateOnly(2024, 1, 5),
            Value = 5000m
        });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculateHoldingsCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 1, 15)));

        // Only cash holding should exist
        var holdings = await _context.Holdings.ToListAsync();
        Assert.Single(holdings);
        Assert.Equal(Instrument.Cash, holdings[0].InstrumentId);
        Assert.Equal(5000m, holdings[0].Units);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
