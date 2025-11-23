namespace Hoard.Core.Domain;

public class PortfolioPerformanceCumulative : PerformanceCumulative
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
}