using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages;
using Hoard.Messages.Positions;
using Rebus.Handlers;

namespace Hoard.Bus.Positions;

public class PositionsCalculatedPropagator(IMediator mediator)
    : IHandleMessages<PositionsCalculatedEvent>
{
    public async Task Handle(PositionsCalculatedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand = new TriggerCalculatePerformanceCommand(null, message.PipelineMode);
            await mediator.SendAsync(appCommand);
        }
    }
}
