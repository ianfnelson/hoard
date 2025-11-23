using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Hoard.Messages.Holdings;
using Hoard.Messages.Positions;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Positions;

public class RebuildPositionsBusHandler(IMediator mediator)
    : IHandleMessages<RebuildPositionsBusCommand>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    public async Task Handle(RebuildPositionsBusCommand message)
    {
        var appCommand = new ProcessRebuildPositionsCommand(message.CorrelationId);
        
        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(HoldingsCalculatedEvent message)
    {
        if (!message.IsBackfill)
        {
            var appCommand = new ProcessRebuildPositionsCommand(message.CorrelationId);

            await mediator.SendAsync(appCommand);
        }
    }
}