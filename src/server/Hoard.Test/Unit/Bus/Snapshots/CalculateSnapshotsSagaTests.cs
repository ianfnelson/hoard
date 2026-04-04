using Hoard.Bus.Snapshots;
using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Snapshots;

public class CalculateSnapshotsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly CalculateSnapshotsSaga _saga;

    public CalculateSnapshotsSagaTests()
    {
        _saga = new CalculateSnapshotsSaga(Mock.Of<ILogger<CalculateSnapshotsSaga>>(), _mediator.Object, _bus.Object)
        {
            Data = new CsSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithPortfolios_DispatchesSnapshots()
    {
        var runId = Guid.NewGuid();
        var portfolioIds = new List<int> { 1, 2 };

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        await _saga.Handle(new StartCalculateSnapshotsSagaCommand(runId, PipelineMode.CloseOfDay, null, 2024));

        Assert.Equal(runId, _saga.Data.SnapshotsRunId);
        Assert.Equal(2024, _saga.Data.Year);
        Assert.Equal(new HashSet<int> { 1, 2 }, _saga.Data.PendingPortfolios);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculateSnapshotCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<SnapshotsCalculatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoPortfolios_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartCalculateSnapshotsSagaCommand(runId, PipelineMode.CloseOfDay, null, 2024));

        _bus.Verify(b => b.Publish(It.Is<SnapshotsCalculatedEvent>(e =>
            e.SnapshotsRunId == runId && e.Year == 2024), null), Times.Once);
    }

    [Fact]
    public async Task Handle_SnapshotCalculated_RemovesPendingPortfolio()
    {
        var runId = Guid.NewGuid();
        _saga.Data.SnapshotsRunId = runId;
        _saga.Data.Year = 2024;
        _saga.Data.PendingPortfolios = new HashSet<int> { 1, 2 };

        await _saga.Handle(new SnapshotCalculatedEvent(runId, PipelineMode.CloseOfDay, 2024, 1));

        Assert.Single(_saga.Data.PendingPortfolios);
        Assert.Contains(2, _saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.IsAny<SnapshotsCalculatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastSnapshotCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.SnapshotsRunId = runId;
        _saga.Data.Year = 2024;
        _saga.Data.PendingPortfolios = new HashSet<int> { 1 };

        await _saga.Handle(new SnapshotCalculatedEvent(runId, PipelineMode.CloseOfDay, 2024, 1));

        Assert.Empty(_saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.Is<SnapshotsCalculatedEvent>(e =>
            e.SnapshotsRunId == runId && e.Year == 2024), null), Times.Once);
    }
}
