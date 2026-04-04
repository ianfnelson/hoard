using Hoard.Bus.Chrono;
using Hoard.Core.Application;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Hoard.Messages.Chrono;
using Hoard.Messages.Holdings;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Chrono;

public class CloseOfDaySagaTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly CloseOfDaySaga _saga;

    public CloseOfDaySagaTests()
    {
        _saga = new CloseOfDaySaga(_bus.Object, _mediator.Object, Mock.Of<ILogger<CloseOfDaySaga>>())
        {
            Data = new ChronoSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_DispatchesPricesAndHoldings()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await _saga.Handle(new StartCloseOfDaySagaCommand(runId, asOfDate, PipelineMode.CloseOfDay));

        Assert.Equal(runId, _saga.Data.ChronoRunId);
        Assert.Equal(PipelineMode.CloseOfDay, _saga.Data.PipelineMode);
        Assert.Equal(asOfDate, _saga.Data.Today);
        Assert.Equal(asOfDate.AddDays(1), _saga.Data.Tomorrow);
        Assert.Equal(asOfDate.AddDays(-7), _saga.Data.RebuildStartDate);

        // Should dispatch both prices and holdings
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_HoldingsBackfilled_WhenPricesNotYetRefreshed_DispatchesPositionsOnly()
    {
        SetupStartedSaga();
        _saga.Data.PricesRefreshed = false;

        await _saga.Handle(new HoldingsBackfilledEvent(_saga.Data.HoldingsRunId, PipelineMode.CloseOfDay, _saga.Data.RebuildStartDate, _saga.Data.Tomorrow));

        Assert.True(_saga.Data.HoldingsCalculated);

        // Should dispatch positions but not valuations (prices not ready)
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingsBackfilled_WhenPricesAlreadyRefreshed_DispatchesPositionsAndValuations()
    {
        SetupStartedSaga();
        _saga.Data.PricesRefreshed = true;

        await _saga.Handle(new HoldingsBackfilledEvent(_saga.Data.HoldingsRunId, PipelineMode.CloseOfDay, _saga.Data.RebuildStartDate, _saga.Data.Tomorrow));

        Assert.True(_saga.Data.HoldingsCalculated);

        // Should dispatch both positions and valuations
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_PricesRefreshed_WhenHoldingsNotYetCalculated_DispatchesAnalysisOnly()
    {
        SetupStartedSaga();
        _saga.Data.HoldingsCalculated = false;

        await _saga.Handle(new PricesRefreshedEvent(_saga.Data.PricesRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.PricesRefreshed);

        // Should dispatch analysis but not valuations (holdings not ready)
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PricesRefreshed_WhenHoldingsAlreadyCalculated_DispatchesValuationsAndAnalysis()
    {
        SetupStartedSaga();
        _saga.Data.HoldingsCalculated = true;

        await _saga.Handle(new PricesRefreshedEvent(_saga.Data.PricesRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.PricesRefreshed);

        // Should dispatch valuations and analysis
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ValuationsBackfilled_WhenPositionsCalculated_DispatchesPerformanceAndSnapshots()
    {
        SetupStartedSaga();
        _saga.Data.PositionsCalculated = true;

        await _saga.Handle(new ValuationsBackfilledEvent(_saga.Data.ValuationsRunId, PipelineMode.CloseOfDay, null, _saga.Data.RebuildStartDate, _saga.Data.Tomorrow));

        Assert.True(_saga.Data.ValuationsCalculated);

        // Should dispatch performance and snapshots
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ValuationsBackfilled_WhenPositionsNotYetCalculated_DispatchesSnapshotsOnly()
    {
        SetupStartedSaga();
        _saga.Data.PositionsCalculated = false;

        await _saga.Handle(new ValuationsBackfilledEvent(_saga.Data.ValuationsRunId, PipelineMode.CloseOfDay, null, _saga.Data.RebuildStartDate, _saga.Data.Tomorrow));

        Assert.True(_saga.Data.ValuationsCalculated);

        // Should dispatch snapshots but not performance
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PositionsCalculated_WhenValuationsCalculated_DispatchesPerformance()
    {
        SetupStartedSaga();
        _saga.Data.ValuationsCalculated = true;

        await _saga.Handle(new PositionsCalculatedEvent(_saga.Data.PositionsRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.PositionsCalculated);
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PositionsCalculated_WhenValuationsNotYetCalculated_DoesNotDispatchPerformance()
    {
        SetupStartedSaga();
        _saga.Data.ValuationsCalculated = false;

        await _saga.Handle(new PositionsCalculatedEvent(_saga.Data.PositionsRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.PositionsCalculated);
        _mediator.Verify(m => m.SendAsync(It.IsAny<ICommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PerformanceCalculated_WhenSnapshotsAndAnalysisComplete_CompletesSaga()
    {
        SetupStartedSaga();
        _saga.Data.SnapshotsCalculated = true;
        _saga.Data.AnalysisCompleted = true;

        await _saga.Handle(new PerformanceCalculatedEvent(_saga.Data.PerformanceRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.PerformanceCalculated);
        _bus.Verify(b => b.Publish(It.Is<CloseOfDayCompletedEvent>(e =>
            e.ChronoRunId == _saga.Data.ChronoRunId), null), Times.Once);
    }

    [Fact]
    public async Task Handle_PerformanceCalculated_WhenSnapshotsNotComplete_DoesNotComplete()
    {
        SetupStartedSaga();
        _saga.Data.SnapshotsCalculated = false;
        _saga.Data.AnalysisCompleted = true;

        await _saga.Handle(new PerformanceCalculatedEvent(_saga.Data.PerformanceRunId, PipelineMode.CloseOfDay));

        _bus.Verify(b => b.Publish(It.IsAny<CloseOfDayCompletedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_SnapshotsCalculated_WhenPerformanceAndAnalysisComplete_CompletesSaga()
    {
        SetupStartedSaga();
        _saga.Data.PerformanceCalculated = true;
        _saga.Data.AnalysisCompleted = true;

        await _saga.Handle(new SnapshotsCalculatedEvent(_saga.Data.SnapshotsRunId, PipelineMode.CloseOfDay, 2024));

        Assert.True(_saga.Data.SnapshotsCalculated);
        _bus.Verify(b => b.Publish(It.Is<CloseOfDayCompletedEvent>(e =>
            e.ChronoRunId == _saga.Data.ChronoRunId), null), Times.Once);
    }

    [Fact]
    public async Task Handle_SnapshotsCalculated_WhenPerformanceNotComplete_DoesNotComplete()
    {
        SetupStartedSaga();
        _saga.Data.PerformanceCalculated = false;
        _saga.Data.AnalysisCompleted = true;

        await _saga.Handle(new SnapshotsCalculatedEvent(_saga.Data.SnapshotsRunId, PipelineMode.CloseOfDay, 2024));

        Assert.True(_saga.Data.SnapshotsCalculated);
        _bus.Verify(b => b.Publish(It.IsAny<CloseOfDayCompletedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_AnalysisCompleted_WhenPerformanceAndSnapshotsComplete_CompletesSaga()
    {
        SetupStartedSaga();
        _saga.Data.PerformanceCalculated = true;
        _saga.Data.SnapshotsCalculated = true;

        await _saga.Handle(new InstrumentsAnalysedEvent(_saga.Data.AnalysisRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.AnalysisCompleted);
        _bus.Verify(b => b.Publish(It.Is<CloseOfDayCompletedEvent>(e =>
            e.ChronoRunId == _saga.Data.ChronoRunId), null), Times.Once);
    }

    [Fact]
    public async Task Handle_AnalysisCompleted_WhenPerformanceNotComplete_DoesNotComplete()
    {
        SetupStartedSaga();
        _saga.Data.PerformanceCalculated = false;
        _saga.Data.SnapshotsCalculated = true;

        await _saga.Handle(new InstrumentsAnalysedEvent(_saga.Data.AnalysisRunId, PipelineMode.CloseOfDay));

        Assert.True(_saga.Data.AnalysisCompleted);
        _bus.Verify(b => b.Publish(It.IsAny<CloseOfDayCompletedEvent>(), null), Times.Never);
    }

    private void SetupStartedSaga()
    {
        _saga.Data.ChronoRunId = Guid.NewGuid();
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.Today = new DateOnly(2024, 6, 15);
        _saga.Data.Tomorrow = new DateOnly(2024, 6, 16);
        _saga.Data.RebuildStartDate = new DateOnly(2024, 6, 8);
    }
}
