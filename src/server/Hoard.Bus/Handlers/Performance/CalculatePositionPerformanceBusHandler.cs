using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performance;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Performance;

public class CalculatePositionPerformanceBusHandler(IMediator mediator)
    : IHandleMessages<CalculatePositionPerformanceBusCommand>
{
    public async Task Handle(CalculatePositionPerformanceBusCommand message)
    {
        var appCommand =
            new ProcessCalculatePositionPerformanceCommand(message.CorrelationId, message.InstrumentId,
                message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }
}