namespace Hoard.Core.Domain;

public class AssetSubclass : Entity<int>
{
    public int AssetClassId { get; set; }
    public AssetClass AssetClass { get; set; } = null!;
    
    public required string Name { get; set; }
    public required string Code { get; set; }
}