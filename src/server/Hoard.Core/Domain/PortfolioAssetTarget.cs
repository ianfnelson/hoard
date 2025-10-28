namespace Hoard.Core.Domain;

public class PortfolioAssetTarget : Entity<int>
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    
    public int AssetSubclassId { get; set; }
    public AssetSubclass AssetSubclass { get; set; } = null!;
    
    public decimal Target { get; set; }
}