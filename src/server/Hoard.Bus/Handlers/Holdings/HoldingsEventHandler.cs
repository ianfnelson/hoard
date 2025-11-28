using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class HoldingsEventHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingsBusCommand>, 
        IHandleMessages<HoldingChangedEvent>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsCommand(message.CorrelationId, message.PipelineMode, message.AsOfDate);
        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(HoldingChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, message.AsOfDate);
        
            await mediator.SendAsync(appCommand);
        }
    }
    
    public async Task Handle(HoldingsCalculatedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand = new ProcessCalculatePositionsCommand(message.CorrelationId, message.PipelineMode);

            await mediator.SendAsync(appCommand);
        }
    }
}