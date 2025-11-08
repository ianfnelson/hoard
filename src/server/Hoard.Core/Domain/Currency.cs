namespace Hoard.Core.Domain;

public partial class Currency : Entity<string>
{
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}