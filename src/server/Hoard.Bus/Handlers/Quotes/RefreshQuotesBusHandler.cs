using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Quotes;

public class RefreshQuotesBusHandler(IMediator mediator) : IHandleMessages<RefreshQuotesBusCommand>
{
    public async Task Handle(RefreshQuotesBusCommand message)
    {
        var applicationCommand = new ProcessRefreshQuotesCommand(message.CorrelationId);
        
        await mediator.SendAsync(applicationCommand);
    }
}