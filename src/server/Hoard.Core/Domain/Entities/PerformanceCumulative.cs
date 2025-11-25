namespace Hoard.Core.Domain.Entities;

public abstract class 
    PerformanceCumulative : Entity<int>
{
    // Valuation snapshot
    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal ValueChange { get; set; }

    // Gains breakdown
    public decimal UnrealisedGain { get; set; }
    public decimal RealisedGain { get; set; }
    public decimal Income { get; set; }

    // Returns (Modified Dietz windows)
    public decimal? Return1D { get; set; }
    public decimal? Return1W { get; set; }
    public decimal? Return1M { get; set; }
    public decimal? Return3M { get; set; }
    public decimal? Return6M { get; set; }
    public decimal? Return1Y { get; set; }
    public decimal? Return3Y { get; set; }
    public decimal? Return5Y { get; set; }
    public decimal? ReturnYtd { get; set; }
    public decimal? ReturnAllTime { get; set; }
    public decimal? AnnualisedReturn { get; set; }

    public DateTime UpdatedUtc { get; set; }
}