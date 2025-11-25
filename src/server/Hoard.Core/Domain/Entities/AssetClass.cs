namespace Hoard.Core.Domain.Entities;

public class AssetClass : Entity<int>
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}