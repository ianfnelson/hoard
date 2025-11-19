namespace Hoard.Core.Domain;

public class HoldingValuation : Entity<int>
{
    public int HoldingId { get; set; }
    public Holding Holding { get; set; } = null!;
    
    public decimal Value { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}