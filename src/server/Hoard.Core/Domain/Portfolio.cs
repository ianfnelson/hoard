namespace Hoard.Core.Domain;

public class Portfolio : Entity<int>
{
    public required string Name { get; set; }
    
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<PortfolioAssetTarget> AssetTargets { get; set; } = new List<PortfolioAssetTarget>();
    
    public bool IsActive { get; set; }
}