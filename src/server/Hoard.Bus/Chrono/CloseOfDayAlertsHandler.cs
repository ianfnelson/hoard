using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.Chrono;
using Rebus.Handlers;

namespace Hoard.Bus.Chrono;

public class CloseOfDayAlertsHandler(IMediator mediator)
    : IHandleMessages<CloseOfDayCompletedEvent>
{
    public async Task Handle(CloseOfDayCompletedEvent message)
    {
        await mediator.SendAsync(new CloseOfDayAlertCommand(message.AsOfDate));
    }
}