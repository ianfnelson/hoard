using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Snapshots;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class ValuationsEventHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingValuationsBusCommand>,
        IHandleMessages<HoldingValuationsChangedEvent>,
        IHandleMessages<CalculatePortfolioValuationBusCommand>,
        IHandleMessages<PortfolioValuationChangedEvent>
{
    public async Task Handle(CalculateHoldingValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingValuationsCommand(message.ValuationsRunId, message.PipelineMode, message.InstrumentId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(CalculatePortfolioValuationBusCommand message)
    {
        var appCommand = new ProcessCalculatePortfolioValuationCommand(message.ValuationsRunId, message.PipelineMode, message.PortfolioId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(HoldingValuationsChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            if (message.AsOfDate == DateOnlyHelper.TodayLocal())
            {
                var positionPerformanceAppCommand =
                    new ProcessCalculatePositionPerformanceCommand(Guid.NewGuid(), message.InstrumentId,
                        message.PipelineMode);

                await mediator.SendAsync(positionPerformanceAppCommand); 
            }
        }
    }
    
    public async Task Handle(PortfolioValuationChangedEvent message)
    {
        if (message.PipelineMode == PipelineMode.DaytimeReactive)
        {
            var impactedYears = new List<int> { message.AsOfDate.Year };
            if (message.AsOfDate is { Month: 12, Day: 31 })
            {
                impactedYears.Add(message.AsOfDate.Year + 1);
            }

            foreach (var year in impactedYears)
            {
                var appCommand = new ProcessCalculateSnapshotCommand(Guid.NewGuid(), message.PipelineMode, message.PortfolioId, year);
                await mediator.SendAsync(appCommand);
            }
        }
    }
}
