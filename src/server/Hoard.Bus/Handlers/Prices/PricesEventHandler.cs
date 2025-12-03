using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class PricesEventHandler(IMediator mediator) : 
    IHandleMessages<RefreshPricesBatchBusCommand>,
    IHandleMessages<PriceChangedEvent>
{
    public async Task Handle(RefreshPricesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshPricesBatchCommand(
            message.CorrelationId,
            message.PipelineMode,
            message.InstrumentId,
            message.StartDate,
            message.EndDate
        );

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(PriceChangedEvent message)
    {
        if (message is { PipelineMode: PipelineMode.DaytimeReactive, IsFxPair: false })
        {
            var appCommand =
                new ProcessCalculateHoldingValuationsCommand(message.CorrelationId, message.PipelineMode, message.InstrumentId, message.AsOfDate);

            await mediator.SendAsync(appCommand);
        }
    }
}