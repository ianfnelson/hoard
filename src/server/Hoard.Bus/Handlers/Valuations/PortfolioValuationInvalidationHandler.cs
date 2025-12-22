using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class PortfolioValuationInvalidationHandler(IBus bus)
    :
        IHandleMessages<HoldingValuationsChangedEvent>
{
    public async Task Handle(HoldingValuationsChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            await bus.Publish(new PortfolioValuationsInvalidatedEvent(message.PipelineMode, message.AsOfDate));
        }
    }
}