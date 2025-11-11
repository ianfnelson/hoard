using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages.Prices;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class RefreshPricesBusHandler(IMediator mediator) 
    : IHandleMessages<RefreshPricesBusCommand>
{
    public async Task Handle(RefreshPricesBusCommand message)
    {
        var appCommand = new ProcessRefreshPricesCommand(message.CorrelationId, message.AsOfDate)
        {
            AsOfDate = message.AsOfDate
        };

        await mediator.SendAsync(appCommand);
    }
}