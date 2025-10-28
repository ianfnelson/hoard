namespace Hoard.Core.Domain;

public class Price : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public DateOnly AsOfDate { get; set; }
    public DateTime RetrievedUtc { get; set; }
    public required string Source { get; set; }
    public decimal Value { get; set; }
}