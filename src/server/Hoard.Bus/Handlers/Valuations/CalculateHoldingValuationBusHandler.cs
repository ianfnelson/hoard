using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateHoldingValuationBusHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingValuationBusCommand>
{
    public async Task Handle(CalculateHoldingValuationBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsValuationCommand(message.CorrelationId, message.HoldingId, message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }
}
