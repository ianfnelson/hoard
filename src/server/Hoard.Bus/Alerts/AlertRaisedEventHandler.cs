using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.Alerts;
using Rebus.Handlers;

namespace Hoard.Bus.Alerts;

public class AlertRaisedEventHandler(IMediator mediator) : IHandleMessages<AlertRaisedEvent>
{
    public async Task Handle(AlertRaisedEvent message)
    {
        var command = new PublishAlertNotificationCommand(
            message.Title, message.Message, message.CreatedUtc, message.Url, message.UrlTitle);
        await mediator.SendAsync(command);
    }
}