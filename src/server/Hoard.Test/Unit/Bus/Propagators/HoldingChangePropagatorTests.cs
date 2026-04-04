using Hoard.Bus.Holdings;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class HoldingChangePropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly HoldingChangePropagator _propagator;

    public HoldingChangePropagatorTests()
    {
        _propagator = new HoldingChangePropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_HoldingChanged_DaytimeReactive_DispatchesHoldingValuations()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await _propagator.Handle(new HoldingChangedEvent(runId, PipelineMode.DaytimeReactive, asOfDate, 1));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingValuationsCommand>(c =>
                c.InstrumentId == 1 && c.AsOfDate == asOfDate),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingChanged_CloseOfDay_DoesNotDispatch()
    {
        await _propagator.Handle(new HoldingChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, new DateOnly(2024, 6, 15), 1));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateHoldingValuationsCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_HoldingsCalculated_DaytimeReactive_Today_DispatchesPositions()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new HoldingsCalculatedEvent(Guid.NewGuid(), PipelineMode.DaytimeReactive, today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePositionsCommand>(c => c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingsCalculated_DaytimeReactive_NotToday_DoesNotDispatchPositions()
    {
        var yesterday = DateOnlyHelper.TodayLocal().AddDays(-1);

        await _propagator.Handle(new HoldingsCalculatedEvent(Guid.NewGuid(), PipelineMode.DaytimeReactive, yesterday));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionsCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_HoldingsCalculated_CloseOfDay_DoesNotDispatchPositions()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new HoldingsCalculatedEvent(Guid.NewGuid(), PipelineMode.CloseOfDay, today));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionsCommand>(), default), Times.Never);
    }
}
