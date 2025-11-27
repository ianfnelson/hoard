using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsBusHandler(IMediator mediator) 
    : IHandleMessages<CalculateValuationsBusCommand>
{
    public async Task Handle(CalculateValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, message.AsOfDate, message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }
}
