namespace Hoard.Core.Domain.Entities;

public class PortfolioPerformance : Performance
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    
    public decimal CashValue { get; set; }
    public decimal Yield { get; set; }
}