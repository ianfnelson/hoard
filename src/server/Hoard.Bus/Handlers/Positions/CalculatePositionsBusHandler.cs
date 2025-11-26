using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Hoard.Messages.Holdings;
using Hoard.Messages.Positions;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Positions;

public class CalculatePositionsBusHandler(IMediator mediator)
    : IHandleMessages<CalculatePositionsBusCommand>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    public async Task Handle(CalculatePositionsBusCommand message)
    {
        var appCommand = new ProcessCalculatePositionsCommand(message.CorrelationId, message.SuppressCascade);
        
        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(HoldingsCalculatedEvent message)
    {
        if (!message.IsBackfill)
        {
            var appCommand = new ProcessCalculatePositionsCommand(message.CorrelationId, false);

            await mediator.SendAsync(appCommand);
        }
    }
}