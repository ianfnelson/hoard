using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages.Prices;
using Rebus.Handlers;

namespace Hoard.Bus.Prices;

public class RefreshPricesCommandHandler(IMediator mediator) :
    IHandleMessages<RefreshPricesBatchBusCommand>
{
    public async Task Handle(RefreshPricesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshPricesBatchCommand(
            message.PricesRunId,
            message.PipelineMode,
            message.InstrumentId,
            message.StartDate,
            message.EndDate
        );

        await mediator.SendAsync(appCommand);
    }
}
