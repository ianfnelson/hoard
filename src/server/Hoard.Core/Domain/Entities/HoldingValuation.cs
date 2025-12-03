namespace Hoard.Core.Domain.Entities;

public class HoldingValuation : Valuation
{
    public int HoldingId { get; set; }
    public Holding Holding { get; set; } = null!;
}