namespace Hoard.Core.Domain;

public class InstrumentValuation : Valuation
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
}