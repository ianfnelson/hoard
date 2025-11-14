namespace Hoard.Core.Domain;

public abstract class Valuation : Entity<int>
{
    public decimal Value { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}