namespace Hoard.Core.Domain.Entities;

public class PositionPerformanceCumulative : PerformanceCumulative
{
    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    // Position-specific metrics
    public decimal CostBasis { get; set; }
    public decimal Units { get; set; }
}