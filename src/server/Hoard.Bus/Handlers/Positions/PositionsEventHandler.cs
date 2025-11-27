using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Hoard.Messages.Positions;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Positions;

public class PositionsEventHandler(IMediator mediator)
    : IHandleMessages<CalculatePositionsBusCommand>
{
    public async Task Handle(CalculatePositionsBusCommand message)
    {
        var appCommand = new ProcessCalculatePositionsCommand(message.CorrelationId, message.PipelineMode);
        
        await mediator.SendAsync(appCommand);
    }
}