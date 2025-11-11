using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Quotes;

public class RefreshQuotesBatchBusHandler(IMediator mediator) : IHandleMessages<RefreshQuotesBatchBusCommand>
{
    public async Task Handle(RefreshQuotesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshQuotesBatchCommand(message.CorrelationId, message.InstrumentIds);
        
        await mediator.SendAsync(appCommand);
    }
}