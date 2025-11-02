namespace Hoard.Core.Domain;

public class Currency : Entity<string>
{
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}