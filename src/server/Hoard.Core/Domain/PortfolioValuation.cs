namespace Hoard.Core.Domain;

public class PortfolioValuation : Valuation
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
}