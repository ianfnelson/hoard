using Hoard.Bus.Valuations;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class ValuationChangePropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly ValuationChangePropagator _propagator;

    public ValuationChangePropagatorTests()
    {
        _propagator = new ValuationChangePropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_DaytimeReactive_Today_DispatchesPositionPerformance()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePositionPerformanceCommand>(c =>
                c.InstrumentId == 1 && c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_DaytimeReactive_NotToday_DoesNotDispatch()
    {
        var yesterday = DateOnlyHelper.TodayLocal().AddDays(-1);

        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, yesterday));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_CloseOfDay_DoesNotDispatch()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 1, today));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_DaytimeReactive_Today_DispatchesSnapshot()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 5, today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateSnapshotCommand>(c =>
                c.PortfolioId == 5 && c.Year == today.Year),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_DaytimeReactive_NotToday_DoesNotDispatch()
    {
        var yesterday = DateOnlyHelper.TodayLocal().AddDays(-1);

        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 5, yesterday));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateSnapshotCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_CloseOfDay_DoesNotDispatch()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 5, today));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateSnapshotCommand>(), default), Times.Never);
    }
}
