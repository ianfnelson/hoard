using Hoard.Messages;
using Hoard.Messages.Performance;
using Hoard.Messages.Valuations;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Performance;

public class PortfolioPerformanceInvalidationHandler(IBus bus)
    :
        IHandleMessages<PositionPerformanceCalculatedEvent>,
        IHandleMessages<PortfolioValuationChangedEvent>
{
    public async Task Handle(PositionPerformanceCalculatedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            await bus.Publish(new PortfolioPerformancesInvalidatedEvent(message.CorrelationId, message.PipelineMode));
        }
    }

    public async Task Handle(PortfolioValuationChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            await bus.Publish(new PortfolioPerformancesInvalidatedEvent(message.CorrelationId, message.PipelineMode));
        }
    }
}