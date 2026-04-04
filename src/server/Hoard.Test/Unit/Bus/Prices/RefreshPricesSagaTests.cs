using Hoard.Bus.Prices;
using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Prices;

public class RefreshPricesSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly RefreshPricesSaga _saga;

    public RefreshPricesSagaTests()
    {
        _saga = new RefreshPricesSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<RefreshPricesSaga>>())
        {
            Data = new RpSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_DispatchesRefreshAndTracksPending()
    {
        var runId = Guid.NewGuid();
        var instrumentIds = new List<int> { 1, 2, 3 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForRefreshQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartRefreshPricesSagaCommand(runId, PipelineMode.CloseOfDay, null, null, null));

        Assert.Equal(runId, _saga.Data.PricesRunId);
        Assert.Equal(PipelineMode.CloseOfDay, _saga.Data.PipelineMode);
        Assert.Equal(3, _saga.Data.PendingInstruments.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchRefreshPricesCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<PricesRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForRefreshQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartRefreshPricesSagaCommand(runId, PipelineMode.CloseOfDay, null, null, null));

        _bus.Verify(b => b.Publish(It.Is<PricesRefreshedEvent>(e =>
            e.PricesRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task Handle_PriceRefreshed_RemovesPendingInstrument()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PricesRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1, 2 };

        await _saga.Handle(new PriceRefreshedEvent(runId, PipelineMode.CloseOfDay, 1, 1,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), DateTime.UtcNow));

        Assert.Single(_saga.Data.PendingInstruments);
        Assert.Contains(2, _saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.IsAny<PricesRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastPriceRefreshed_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PricesRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1 };

        await _saga.Handle(new PriceRefreshedEvent(runId, PipelineMode.CloseOfDay, 1, 1,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), DateTime.UtcNow));

        Assert.Empty(_saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.Is<PricesRefreshedEvent>(e =>
            e.PricesRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }
}
