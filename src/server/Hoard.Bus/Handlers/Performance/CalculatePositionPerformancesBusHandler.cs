using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performances;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Performance;

public class CalculatePositionPerformancesBusHandler(IMediator mediator)
    : IHandleMessages<CalculatePositionPerformancesBusCommand>,
        IHandleMessages<ValuationChangedEvent>
{
    public async Task Handle(CalculatePositionPerformancesBusCommand message)
    {
        var appCommand =
            new ProcessCalculatePositionPerformancesCommand(message.CorrelationId, message.InstrumentId,
                message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(ValuationChangedEvent message)
    {
        if (message.AsOfDate == DateOnlyHelper.TodayLocal())
        {
            var appCommand =
                new ProcessCalculatePositionPerformancesCommand(message.CorrelationId, message.InstrumentId,
                    false);

            await mediator.SendAsync(appCommand);
        }
    }
}