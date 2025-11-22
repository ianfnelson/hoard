namespace Hoard.Core.Domain;

public class PositionPerformanceCumulative : PerformanceCumulative
{
    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    // How much of the portfolio this position represents
    public decimal PortfolioWeightPercent { get; set; }

    // Position-specific metrics
    public decimal CostBasis { get; set; }
    public decimal Units { get; set; }
}