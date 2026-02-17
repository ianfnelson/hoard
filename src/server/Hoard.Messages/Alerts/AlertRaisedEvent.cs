namespace Hoard.Messages.Alerts;

public record AlertRaisedEvent(
    int AlertTypeId,
    DateOnly TriggeredDate,
    DateTime CreatedUtc,
    string Title,
    string Message,
    string? Url = null,
    string? UrlTitle = null
    );