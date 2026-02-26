using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Quotes;

public class QuoteChangedAlertEventHandler(IMediator mediator)
    : IHandleMessages<QuoteChangedEvent>
{
    public async Task Handle(QuoteChangedEvent message)
    {
        if (message.RegularMarketChangePercent is null)
            return;

        await mediator.SendAsync(new QuoteAlertCommand(
            message.InstrumentId,
            message.RegularMarketChangePercent.Value));
    }
}
