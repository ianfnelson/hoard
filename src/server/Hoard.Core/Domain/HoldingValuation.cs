namespace Hoard.Core.Domain;

public class HoldingValuation : Valuation
{
    public int HoldingId { get; set; }
    public Holding Holding { get; set; } = null!;
}