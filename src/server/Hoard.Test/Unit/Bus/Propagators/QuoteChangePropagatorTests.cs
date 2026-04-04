using Hoard.Bus.Quotes;
using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Quotes;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class QuoteChangePropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly QuoteChangePropagator _propagator;

    public QuoteChangePropagatorTests()
    {
        _propagator = new QuoteChangePropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_NonFxInstrument_DispatchesHoldingValuations()
    {
        var retrievedUtc = DateTime.UtcNow;
        var expectedDate = DateOnly.FromDateTime(retrievedUtc.ToLocalTime());

        await _propagator.Handle(new QuoteChangedEvent(42, InstrumentType.Shares, retrievedUtc, 1.5m));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingValuationsCommand>(c =>
                c.InstrumentId == 42 && c.AsOfDate == expectedDate),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_FxPair_DoesNotDispatch()
    {
        await _propagator.Handle(new QuoteChangedEvent(42, InstrumentType.FxPair, DateTime.UtcNow, 1.5m));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateHoldingValuationsCommand>(), default), Times.Never);
    }
}
