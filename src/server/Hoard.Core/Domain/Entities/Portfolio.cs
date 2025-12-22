namespace Hoard.Core.Domain.Entities;

public class Portfolio : Entity<int>
{
    public required string Name { get; set; }
    
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<TargetAllocation> AssetTargets { get; set; } = new List<TargetAllocation>();
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}