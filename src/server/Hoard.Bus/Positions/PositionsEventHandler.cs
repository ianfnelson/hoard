using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Positions;
using Hoard.Messages;
using Hoard.Messages.Positions;
using Rebus.Handlers;

namespace Hoard.Bus.Positions;

public class PositionsEventHandler(IMediator mediator)
    : 
        IHandleMessages<CalculatePositionsBusCommand>,
        IHandleMessages<PositionsCalculatedEvent>
{
    public async Task Handle(CalculatePositionsBusCommand message)
    {
        var appCommand = new ProcessCalculatePositionsCommand(message.PositionsRunId, message.PipelineMode);
        
        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(PositionsCalculatedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand = new TriggerCalculatePerformanceCommand(null, message.PipelineMode);
            await mediator.SendAsync(appCommand);
        }
    }
}