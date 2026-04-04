using Hoard.Bus.Quotes;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Quotes;
using Moq;

namespace Hoard.Test.Unit.Bus.Quotes;

public class QuoteChangedAlertEventHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly QuoteChangedAlertEventHandler _handler;

    public QuoteChangedAlertEventHandlerTests()
    {
        _handler = new QuoteChangedAlertEventHandler(_mediator.Object);
    }

    [Fact]
    public async Task Handle_WithChangePercent_DispatchesQuoteAlertCommand()
    {
        await _handler.Handle(new QuoteChangedEvent(42, InstrumentType.Shares, DateTime.UtcNow, 5.5m));

        _mediator.Verify(m => m.SendAsync(
            It.Is<QuoteAlertCommand>(c =>
                c.InstrumentId == 42 &&
                c.RegularMarketChangePercent == 5.5m),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullChangePercent_DoesNotDispatch()
    {
        await _handler.Handle(new QuoteChangedEvent(42, InstrumentType.Shares, DateTime.UtcNow, null));

        _mediator.Verify(m => m.SendAsync(It.IsAny<QuoteAlertCommand>(), default), Times.Never);
    }
}
