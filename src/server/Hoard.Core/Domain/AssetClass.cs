namespace Hoard.Core.Domain;

public class AssetClass : Entity<int>
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
}