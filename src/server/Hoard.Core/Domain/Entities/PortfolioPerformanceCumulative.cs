namespace Hoard.Core.Domain.Entities;

public class PortfolioPerformanceCumulative : PerformanceCumulative
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    
    public decimal CashValue { get; set; }
    public decimal CashWeightPercent { get; set; }
}