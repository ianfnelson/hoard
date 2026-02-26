namespace Hoard.Core.Domain.Entities;

public class InstrumentAnalysis : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;

    public DateOnly AsOfDate { get; set; }

    public decimal? FiftyTwoWeekHighClose { get; set; }
    public decimal? FiftyTwoWeekLowClose { get; set; }
    public long? ThirtyDayMedianVolume { get; set; }
    public decimal? FiftyTwoWeekVolatility { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
