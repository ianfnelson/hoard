namespace Hoard.Core.Domain.Entities;

public class PortfolioValuation : Valuation
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public DateOnly AsOfDate { get; set; }
}