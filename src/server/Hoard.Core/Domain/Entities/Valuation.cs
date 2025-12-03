namespace Hoard.Core.Domain.Entities;

public abstract class Valuation : Entity<int>
{
    public decimal Value { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}