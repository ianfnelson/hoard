using Hoard.Core.Infrastructure.Integrations;

namespace Hoard.Core.Application.Alerts;

public record PublishAlertNotificationCommand(
    string Title,
    string Message,
    DateTime CreatedUtc,
    string? Url,
    string? UrlTitle
) : ICommand;

public class PublishAlertNotificationHandler(INotificationService notificationService) : ICommandHandler<PublishAlertNotificationCommand>
{
    public async Task HandleAsync(PublishAlertNotificationCommand command, CancellationToken ct = default)
    {
        var notificationDto = new NotificationDto
        {
            Title = command.Title,
            Message = command.Message,
            CreatedUtc = command.CreatedUtc,
            Url = command.Url,
            UrlTitle = command.UrlTitle
        };
        
        await notificationService.SendNotificationAsync(notificationDto);
    }
}