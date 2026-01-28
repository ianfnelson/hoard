namespace Hoard.Core.Domain.Entities;

public class AssetClass : Entity<int>
{
    public required string Name { get; set; }
    
    public ICollection<AssetSubclass> Subclasses { get; set; } = new List<AssetSubclass>();
}