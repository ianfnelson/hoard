using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages.Holdings;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class CalculateHoldingsBusHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingsBusCommand>
{
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsCommand(message.CorrelationId, message.AsOfDate, message.IsBackfill);
        await mediator.SendAsync(appCommand);
    }
}