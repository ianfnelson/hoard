namespace Hoard.Core.Domain.Entities;

public partial class AssetSubclass : Entity<int>
{
    public int AssetClassId { get; set; }
    public AssetClass AssetClass { get; set; } = null!;
    
    public required string Name { get; set; }
}