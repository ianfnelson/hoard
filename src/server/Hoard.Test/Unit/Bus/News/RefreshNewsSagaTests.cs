using Hoard.Bus.News;
using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Hoard.Messages;
using Hoard.Messages.News;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.News;

public class RefreshNewsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly RefreshNewsSaga _saga;

    public RefreshNewsSagaTests()
    {
        _saga = new RefreshNewsSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<RefreshNewsSaga>>())
        {
            Data = new RnSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_DispatchesRefreshAndTracksPending()
    {
        var runId = Guid.NewGuid();
        var instrumentIds = new List<int> { 1, 2, 3 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForNewsRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForNewsRefreshQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartRefreshNewsSagaCommand(runId, PipelineMode.CloseOfDay, null));

        Assert.Equal(runId, _saga.Data.NewsRunId);
        Assert.Equal(PipelineMode.CloseOfDay, _saga.Data.PipelineMode);
        Assert.Equal(3, _saga.Data.PendingInstruments.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchRefreshNewsCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<AllNewsRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForNewsRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForNewsRefreshQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartRefreshNewsSagaCommand(runId, PipelineMode.CloseOfDay, null));

        _bus.Verify(b => b.Publish(It.Is<AllNewsRefreshedEvent>(e =>
            e.NewsRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task Handle_NewsRefreshed_RemovesPendingInstrument()
    {
        var runId = Guid.NewGuid();
        _saga.Data.NewsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1, 2 };

        await _saga.Handle(new NewsRefreshedEvent(runId, PipelineMode.CloseOfDay, 1, DateTime.UtcNow));

        Assert.Single(_saga.Data.PendingInstruments);
        Assert.Contains(2, _saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.IsAny<AllNewsRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastNewsRefreshed_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.NewsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1 };

        await _saga.Handle(new NewsRefreshedEvent(runId, PipelineMode.CloseOfDay, 1, DateTime.UtcNow));

        Assert.Empty(_saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.Is<AllNewsRefreshedEvent>(e =>
            e.NewsRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }
}
