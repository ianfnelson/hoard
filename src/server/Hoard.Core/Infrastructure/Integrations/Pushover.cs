namespace Hoard.Core.Infrastructure.Integrations;

public class NotificationDto
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Url { get; set; }
    public string? UrlTitle { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public interface INotificationService
{
    Task SendNotificationAsync(NotificationDto notification);
}

public class Pushover(HttpClient httpClient, string token, string user) : INotificationService
{
    public async Task SendNotificationAsync(NotificationDto notification)
    {
        var payload = new Dictionary<string, string>
        {
            ["token"] = token,
            ["user"] = user,
            ["title"] = notification.Title,
            ["message"] = notification.Message,
            ["timestamp"] = new DateTimeOffset(notification.CreatedUtc).ToUnixTimeSeconds().ToString()
        };

        if (!string.IsNullOrEmpty(notification.Url))
        {
            payload["url"] = notification.Url;
        }

        if (!string.IsNullOrEmpty(notification.UrlTitle))
        {
            payload["url_title"] = notification.UrlTitle;
        }

        var content = new FormUrlEncodedContent(payload);
        var response = await httpClient.PostAsync("https://api.pushover.net/1/messages.json", content);

        response.EnsureSuccessStatusCode();
    }
}