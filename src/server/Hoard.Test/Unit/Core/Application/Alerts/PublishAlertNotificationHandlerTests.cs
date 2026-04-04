using Hoard.Core.Application.Alerts;
using Hoard.Core.Infrastructure.Integrations;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class PublishAlertNotificationHandlerTests
{
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly PublishAlertNotificationHandler _handler;

    public PublishAlertNotificationHandlerTests()
    {
        _handler = new PublishAlertNotificationHandler(_notificationService.Object);
    }

    [Fact]
    public async Task HandleAsync_SendsNotificationWithCorrectFields()
    {
        var createdUtc = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        await _handler.HandleAsync(new PublishAlertNotificationCommand(
            "Test Title", "Test Message", createdUtc, "http://example.com", "Link"));

        _notificationService.Verify(n => n.SendNotificationAsync(It.Is<NotificationDto>(dto =>
            dto.Title == "Test Title" &&
            dto.Message == "Test Message" &&
            dto.CreatedUtc == createdUtc &&
            dto.Url == "http://example.com" &&
            dto.UrlTitle == "Link")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_HandlesNullUrlFields()
    {
        await _handler.HandleAsync(new PublishAlertNotificationCommand(
            "Title", "Message", DateTime.UtcNow, null, null));

        _notificationService.Verify(n => n.SendNotificationAsync(It.Is<NotificationDto>(dto =>
            dto.Url == null && dto.UrlTitle == null)), Times.Once);
    }
}
