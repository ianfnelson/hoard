namespace Hoard.Core.Domain;

public abstract class Valuation : Entity<int>
{
    
    
    public decimal ValuationGbp { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}