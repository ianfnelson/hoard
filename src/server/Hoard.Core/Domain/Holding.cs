namespace Hoard.Core.Domain;

public class Holding : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public DateOnly AsOfDate { get; set; }
    public decimal Units { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
    
    public Valuation? Valuation { get; set; }
}