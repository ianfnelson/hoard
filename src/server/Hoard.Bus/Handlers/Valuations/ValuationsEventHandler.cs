using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class ValuationsEventHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingValuationsBusCommand>,
        IHandleMessages<HoldingValuationsChangedEvent>,
        IHandleMessages<CalculatePortfolioValuationBusCommand>
{
    public async Task Handle(CalculateHoldingValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingValuationsCommand(message.CorrelationId, message.PipelineMode, message.InstrumentId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(CalculatePortfolioValuationBusCommand message)
    {
        var appCommand = new ProcessCalculatePortfolioValuationCommand(message.CorrelationId, message.PipelineMode, message.PortfolioId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(HoldingValuationsChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            if (message.AsOfDate == DateOnlyHelper.TodayLocal())
            {
                var positionPerformanceAppCommand =
                    new ProcessCalculatePositionPerformanceCommand(message.CorrelationId, message.InstrumentId,
                        message.PipelineMode);

                await mediator.SendAsync(positionPerformanceAppCommand); 
            }
        }
    }
}
