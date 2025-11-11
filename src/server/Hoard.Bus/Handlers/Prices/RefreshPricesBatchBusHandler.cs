using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages.Prices;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class RefreshPricesBatchBusHandler(IMediator mediator) : IHandleMessages<RefreshPricesBatchBusCommand>
{
    public async Task Handle(RefreshPricesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshPricesBatchCommand(
            message.CorrelationId,
            message.InstrumentId,
            message.StartDate,
            message.EndDate
        );

        await mediator.SendAsync(appCommand);
    }
}