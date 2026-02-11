using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Rebus.Handlers;

namespace Hoard.Bus.Holdings;

public class HoldingsEventHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingsBusCommand>, 
        IHandleMessages<HoldingChangedEvent>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsCommand(message.HoldingsRunId, message.PipelineMode, message.AsOfDate);
        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(HoldingChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand = new ProcessCalculateHoldingValuationsCommand(message.HoldingsRunId, message.PipelineMode, message.InstrumentId, message.AsOfDate);
        
            await mediator.SendAsync(appCommand);
        }
    }
    
    public async Task Handle(HoldingsCalculatedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            if (message.AsOfDate == DateOnlyHelper.TodayLocal())
            {
                var appCommand = new ProcessCalculatePositionsCommand(Guid.NewGuid(), message.PipelineMode);
                await mediator.SendAsync(appCommand);
            }
        }
    }
}