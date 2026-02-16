using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Rebus.Handlers;

namespace Hoard.Bus.Prices;

public class PriceChangePropagator(IMediator mediator) :
    IHandleMessages<PriceChangedEvent>
{
    public async Task Handle(PriceChangedEvent message)
    {
        if (message is { PipelineMode: PipelineMode.DaytimeReactive, InstrumentTypeId: not InstrumentType.FxPair })
        {
            var appCommand =
                new ProcessCalculateHoldingValuationsCommand(Guid.NewGuid(), message.PipelineMode, message.InstrumentId, message.AsOfDate);

            await mediator.SendAsync(appCommand);
        }
    }
}
