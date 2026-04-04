using Hoard.Bus.Prices;
using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class PriceChangePropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly PriceChangePropagator _propagator;

    public PriceChangePropagatorTests()
    {
        _propagator = new PriceChangePropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_DaytimeReactive_NonFxInstrument_DispatchesHoldingValuations()
    {
        var asOfDate = new DateOnly(2024, 6, 15);

        await _propagator.Handle(new PriceChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, InstrumentType.Shares, asOfDate, DateTime.UtcNow));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingValuationsCommand>(c =>
                c.InstrumentId == 1 && c.AsOfDate == asOfDate),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_DaytimeReactive_FxPair_DoesNotDispatch()
    {
        await _propagator.Handle(new PriceChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, InstrumentType.FxPair, new DateOnly(2024, 6, 15), DateTime.UtcNow));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateHoldingValuationsCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_CloseOfDay_DoesNotDispatch()
    {
        await _propagator.Handle(new PriceChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 1, InstrumentType.Shares, new DateOnly(2024, 6, 15), DateTime.UtcNow));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateHoldingValuationsCommand>(), default), Times.Never);
    }
}
