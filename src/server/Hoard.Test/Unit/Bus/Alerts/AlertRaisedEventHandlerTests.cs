using Hoard.Bus.Alerts;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.Alerts;
using Moq;

namespace Hoard.Test.Unit.Bus.Alerts;

public class AlertRaisedEventHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AlertRaisedEventHandler _handler;

    public AlertRaisedEventHandlerTests()
    {
        _handler = new AlertRaisedEventHandler(_mediator.Object);
    }

    [Fact]
    public async Task Handle_DispatchesPublishAlertNotificationCommand()
    {
        var createdUtc = DateTime.UtcNow;

        await _handler.Handle(new AlertRaisedEvent(
            1, new DateOnly(2024, 6, 15), createdUtc,
            "Test Title", "Test Message", "https://example.com", "Example"));

        _mediator.Verify(m => m.SendAsync(
            It.Is<PublishAlertNotificationCommand>(c =>
                c.Title == "Test Title" &&
                c.Message == "Test Message" &&
                c.CreatedUtc == createdUtc &&
                c.Url == "https://example.com" &&
                c.UrlTitle == "Example"),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullUrlFields_DispatchesCommand()
    {
        await _handler.Handle(new AlertRaisedEvent(
            1, new DateOnly(2024, 6, 15), DateTime.UtcNow,
            "Title", "Message"));

        _mediator.Verify(m => m.SendAsync(
            It.Is<PublishAlertNotificationCommand>(c =>
                c.Url == null && c.UrlTitle == null),
            default), Times.Once);
    }
}
