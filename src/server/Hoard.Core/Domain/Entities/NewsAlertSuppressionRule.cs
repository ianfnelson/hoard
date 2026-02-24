namespace Hoard.Core.Domain.Entities;

public class NewsAlertSuppressionRule : Entity<int>
{
    public required string HeadlineContains { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}