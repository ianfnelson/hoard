using Hoard.Bus.Positions;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages;
using Hoard.Messages.Positions;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class PositionsCalculatedPropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly PositionsCalculatedPropagator _propagator;

    public PositionsCalculatedPropagatorTests()
    {
        _propagator = new PositionsCalculatedPropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_DaytimeReactive_TriggersPerformanceCalculation()
    {
        await _propagator.Handle(new PositionsCalculatedEvent(Guid.NewGuid(), PipelineMode.DaytimeReactive));

        _mediator.Verify(m => m.SendAsync(
            It.Is<TriggerCalculatePerformanceCommand>(c => c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_CloseOfDay_DoesNotTriggerPerformance()
    {
        await _propagator.Handle(new PositionsCalculatedEvent(Guid.NewGuid(), PipelineMode.CloseOfDay));

        _mediator.Verify(m => m.SendAsync(It.IsAny<TriggerCalculatePerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_Backfill_DoesNotTriggerPerformance()
    {
        await _propagator.Handle(new PositionsCalculatedEvent(Guid.NewGuid(), PipelineMode.Backfill));

        _mediator.Verify(m => m.SendAsync(It.IsAny<TriggerCalculatePerformanceCommand>(), default), Times.Never);
    }
}
