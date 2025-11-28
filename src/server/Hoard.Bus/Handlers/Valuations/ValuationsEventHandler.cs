using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class ValuationsEventHandler(IMediator mediator) 
    : IHandleMessages<CalculateValuationsBusCommand>,
        IHandleMessages<ValuationChangedEvent>
{
    public async Task Handle(CalculateValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.PipelineMode, message.InstrumentId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(ValuationChangedEvent message)
    {
        if (message.AsOfDate == DateOnlyHelper.TodayLocal() && message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var appCommand =
                new ProcessCalculatePositionPerformanceCommand(message.CorrelationId, message.InstrumentId,
                    false);

            await mediator.SendAsync(appCommand);
        }
    }
}
