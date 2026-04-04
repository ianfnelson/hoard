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

public class ProcessCalculatePortfolioValuationHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly ProcessCalculatePortfolioValuationHandler _handler;

    public ProcessCalculatePortfolioValuationHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessCalculatePortfolioValuationHandler(
            _bus.Object,
            NullLogger<ProcessCalculatePortfolioValuationHandler>.Instance,
            _context);
    }

    [Fact]
    public async Task HandleAsync_SumsHoldingValuations()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var asOfDate = new DateOnly(2024, 1, 15);

        var h1 = new Holding { AccountId = account.Id, InstrumentId = 1, AsOfDate = asOfDate, Units = 10, UpdatedUtc = DateTime.UtcNow };
        var h2 = new Holding { AccountId = account.Id, InstrumentId = 2, AsOfDate = asOfDate, Units = 5, UpdatedUtc = DateTime.UtcNow };
        _context.Holdings.AddRange(h1, h2);
        await _context.SaveChangesAsync();

        _context.HoldingValuations.Add(new HoldingValuation { HoldingId = h1.Id, Value = 1000m });
        _context.HoldingValuations.Add(new HoldingValuation { HoldingId = h2.Id, Value = 500m });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        var pv = await _context.PortfolioValuations.SingleAsync();
        Assert.Equal(1500m, pv.Value);
        Assert.Equal(portfolio.Id, pv.PortfolioId);
        Assert.Equal(asOfDate, pv.AsOfDate);
    }

    [Fact]
    public async Task HandleAsync_NoHoldingValuations_CreatesZeroValuation()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var asOfDate = new DateOnly(2024, 1, 15);

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        var pv = await _context.PortfolioValuations.SingleAsync();
        Assert.Equal(0m, pv.Value);
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingValuation_WhenValueChanges()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var asOfDate = new DateOnly(2024, 1, 15);

        _context.PortfolioValuations.Add(new PortfolioValuation
        {
            PortfolioId = portfolio.Id, AsOfDate = asOfDate, Value = 900m, UpdatedUtc = DateTime.UtcNow
        });

        var h1 = new Holding { AccountId = account.Id, InstrumentId = 1, AsOfDate = asOfDate, Units = 10, UpdatedUtc = DateTime.UtcNow };
        _context.Holdings.Add(h1);
        await _context.SaveChangesAsync();

        _context.HoldingValuations.Add(new HoldingValuation { HoldingId = h1.Id, Value = 1000m });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        var pvs = await _context.PortfolioValuations.ToListAsync();
        Assert.Single(pvs);
        Assert.Equal(1000m, pvs[0].Value);
    }

    [Fact]
    public async Task HandleAsync_PublishesChangedEvent_WhenValueChanges()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var asOfDate = new DateOnly(2024, 1, 15);
        var runId = Guid.NewGuid();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            runId, PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        _bus.Verify(b => b.Publish(It.Is<PortfolioValuationChangedEvent>(e =>
            e.ValuationsRunId == runId && e.PortfolioId == portfolio.Id), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DoesNotPublishChangedEvent_WhenValueUnchanged()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        var asOfDate = new DateOnly(2024, 1, 15);

        _context.PortfolioValuations.Add(new PortfolioValuation
        {
            PortfolioId = portfolio.Id, AsOfDate = asOfDate, Value = 0m, UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioValuationChangedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AlwaysPublishesCalculatedEvent()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var asOfDate = new DateOnly(2024, 1, 15);
        var runId = Guid.NewGuid();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            runId, PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        _bus.Verify(b => b.Publish(It.Is<PortfolioValuationCalculatedEvent>(e =>
            e.ValuationsRunId == runId), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_OnlyIncludesHoldingsFromPortfolioAccounts()
    {
        var portfolio = TestData.Portfolio();
        var account = TestData.Account();
        portfolio.Accounts.Add(account);
        _context.Portfolios.Add(portfolio);

        // Account in a different portfolio
        var otherAccount = TestData.Account(id: 99, name: "Other Account");
        _context.Accounts.Add(otherAccount);

        var asOfDate = new DateOnly(2024, 1, 15);

        var h1 = new Holding { AccountId = account.Id, InstrumentId = 1, AsOfDate = asOfDate, Units = 10, UpdatedUtc = DateTime.UtcNow };
        var h2 = new Holding { AccountId = otherAccount.Id, InstrumentId = 2, AsOfDate = asOfDate, Units = 100, UpdatedUtc = DateTime.UtcNow };
        _context.Holdings.AddRange(h1, h2);
        await _context.SaveChangesAsync();

        _context.HoldingValuations.Add(new HoldingValuation { HoldingId = h1.Id, Value = 500m });
        _context.HoldingValuations.Add(new HoldingValuation { HoldingId = h2.Id, Value = 9999m });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessCalculatePortfolioValuationCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, portfolio.Id, asOfDate));

        var pv = await _context.PortfolioValuations.SingleAsync();
        Assert.Equal(500m, pv.Value);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
