using Hoard.Bus.Valuations;
using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Valuations;

public class CalculateValuationsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly CalculateValuationsSaga _saga;

    public CalculateValuationsSagaTests()
    {
        _saga = new CalculateValuationsSaga(Mock.Of<ILogger<CalculateValuationsSaga>>(), _mediator.Object, _bus.Object)
        {
            Data = new CvSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_DispatchesHoldingValuations()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);
        var instrumentIds = new List<int> { 1, 2, 3 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForHoldingValuationsQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForHoldingValuationsQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartCalculateValuationsSagaCommand(runId, PipelineMode.CloseOfDay, null, asOfDate));

        Assert.Equal(runId, _saga.Data.ValuationsRunId);
        Assert.Equal(asOfDate, _saga.Data.AsOfDate);
        Assert.Equal(3, _saga.Data.PendingInstruments.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculateHoldingValuationsCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<ValuationsCalculatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_CompletesImmediately()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForHoldingValuationsQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForHoldingValuationsQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartCalculateValuationsSagaCommand(runId, PipelineMode.CloseOfDay, null, asOfDate));

        _bus.Verify(b => b.Publish(It.Is<ValuationsCalculatedEvent>(e =>
            e.ValuationsRunId == runId && e.AsOfDate == asOfDate), null), Times.Once);
    }

    [Fact]
    public async Task Handle_LastHoldingValuationsCalculated_DispatchesPortfolioValuations()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);
        _saga.Data.ValuationsRunId = runId;
        _saga.Data.AsOfDate = asOfDate;
        _saga.Data.PendingInstruments = new HashSet<int> { 1 };

        var portfolioIds = new List<int> { 10, 20 };
        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        await _saga.Handle(new HoldingValuationsCalculatedEvent(runId, PipelineMode.CloseOfDay, 1, asOfDate));

        Assert.Empty(_saga.Data.PendingInstruments);
        Assert.Equal(new HashSet<int> { 10, 20 }, _saga.Data.PendingPortfolios);
        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePortfolioValuationCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingValuationsCalculated_NotLast_DoesNotDispatchPortfolio()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PendingInstruments = new HashSet<int> { 1, 2 };

        await _saga.Handle(new HoldingValuationsCalculatedEvent(runId, PipelineMode.CloseOfDay, 1, new DateOnly(2024, 6, 15)));

        Assert.Single(_saga.Data.PendingInstruments);
        _mediator.Verify(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
            It.IsAny<GetPortfolioIdsQuery>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_LastPortfolioValuationCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);
        _saga.Data.ValuationsRunId = runId;
        _saga.Data.PendingPortfolios = new HashSet<int> { 10 };

        await _saga.Handle(new PortfolioValuationCalculatedEvent(runId, PipelineMode.CloseOfDay, 10, asOfDate));

        Assert.Empty(_saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.Is<ValuationsCalculatedEvent>(e =>
            e.ValuationsRunId == runId && e.AsOfDate == asOfDate), null), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioValuationCalculated_NotLast_DoesNotComplete()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PendingPortfolios = new HashSet<int> { 10, 20 };

        await _saga.Handle(new PortfolioValuationCalculatedEvent(runId, PipelineMode.CloseOfDay, 10, new DateOnly(2024, 6, 15)));

        Assert.Single(_saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.IsAny<ValuationsCalculatedEvent>(), null), Times.Never);
    }
}
