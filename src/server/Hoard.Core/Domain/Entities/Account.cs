namespace Hoard.Core.Domain.Entities;

public class Account : Entity<int>
{
    public required string Name { get; set; }
    
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();

    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}