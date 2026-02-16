using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Valuations;

public class ValuationChangePropagator(IMediator mediator)
    : IHandleMessages<HoldingValuationsChangedEvent>,
        IHandleMessages<PortfolioValuationChangedEvent>
{
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
            if (message.AsOfDate == DateOnlyHelper.TodayLocal())
            {
                var appCommand = new ProcessCalculateSnapshotCommand(Guid.NewGuid(), message.PipelineMode, message.PortfolioId, message.AsOfDate.Year);
                await mediator.SendAsync(appCommand);
            }
        }
    }
}
