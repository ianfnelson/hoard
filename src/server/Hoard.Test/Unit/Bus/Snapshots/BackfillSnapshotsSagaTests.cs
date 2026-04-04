using Hoard.Bus.Snapshots;
using Hoard.Core.Application;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Snapshots;

public class BackfillSnapshotsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly BackfillSnapshotsSaga _saga;

    public BackfillSnapshotsSagaTests()
    {
        _saga = new BackfillSnapshotsSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<BackfillSnapshotsSaga>>())
        {
            Data = new BfSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_QueriesYearsAndDispatchesBackfill()
    {
        var runId = Guid.NewGuid();
        var years = new List<int> { 2022, 2023, 2024 };

        _mediator.Setup(m => m.QueryAsync<GetYearsForBackfillQuery, IReadOnlyList<int>>(
                It.IsAny<GetYearsForBackfillQuery>(), default))
            .ReturnsAsync(years);

        await _saga.Handle(new StartBackfillSnapshotsSagaCommand(runId, PipelineMode.Backfill, null, 2022, 2024));

        Assert.Equal(runId, _saga.Data.SnapshotsRunId);
        Assert.Equal(PipelineMode.Backfill, _saga.Data.PipelineMode);
        Assert.Equal(2022, _saga.Data.StartYear);
        Assert.Equal(2024, _saga.Data.EndYear);
        Assert.Equal(3, _saga.Data.PendingYears.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchBackfillSnapshotsCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_SnapshotsCalculated_RemovesPendingYear()
    {
        var runId = Guid.NewGuid();
        _saga.Data.SnapshotsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.PendingYears = new HashSet<int> { 2023, 2024 };

        await _saga.Handle(new SnapshotsCalculatedEvent(runId, PipelineMode.Backfill, 2023));

        Assert.Single(_saga.Data.PendingYears);
        Assert.Contains(2024, _saga.Data.PendingYears);
        _bus.Verify(b => b.Publish(It.IsAny<SnapshotsBackfilledEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastSnapshotsCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.SnapshotsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.PendingYears = new HashSet<int> { 2024 };

        await _saga.Handle(new SnapshotsCalculatedEvent(runId, PipelineMode.Backfill, 2024));

        Assert.Empty(_saga.Data.PendingYears);
        _bus.Verify(b => b.Publish(It.Is<SnapshotsBackfilledEvent>(e =>
            e.SnapshotsRunId == runId && e.PipelineMode == PipelineMode.Backfill), null), Times.Once);
    }
}
