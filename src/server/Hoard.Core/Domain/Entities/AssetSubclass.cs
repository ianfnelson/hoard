namespace Hoard.Core.Domain.Entities;

public class AssetSubclass : Entity<int>
{
    public int AssetClassId { get; set; }
    public AssetClass AssetClass { get; set; } = null!;
    
    public required string Name { get; set; }
    public required string Code { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}