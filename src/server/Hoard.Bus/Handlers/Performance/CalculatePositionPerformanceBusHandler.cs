using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performance;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Performance;

public class CalculatePositionPerformanceBusHandler(IMediator mediator)
    : IHandleMessages<CalculatePositionPerformanceBusCommand>,
        IHandleMessages<ValuationChangedEvent>
{
    public async Task Handle(CalculatePositionPerformanceBusCommand message)
    {
        var appCommand =
            new ProcessCalculatePositionPerformanceCommand(message.CorrelationId, message.InstrumentId,
                message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(ValuationChangedEvent message)
    {
        if (message.AsOfDate == DateOnlyHelper.TodayLocal())
        {
            var appCommand =
                new ProcessCalculatePositionPerformanceCommand(message.CorrelationId, message.InstrumentId,
                    false);

            await mediator.SendAsync(appCommand);
        }
    }
}