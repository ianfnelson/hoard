using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Quotes;

public class RefreshQuotesCommandHandler(IMediator mediator) :
    IHandleMessages<RefreshQuotesBatchBusCommand>
{
    public async Task Handle(RefreshQuotesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshQuotesBatchCommand(message.QuotesRunId, message.BatchIndex, message.InstrumentIds);

        await mediator.SendAsync(appCommand);
    }
}
