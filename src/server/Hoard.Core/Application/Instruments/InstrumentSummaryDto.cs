namespace Hoard.Core.Application.Instruments;

public class InstrumentSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public required string TickerDisplay { get; set; }
    public required string Isin { get; set; }
    
    public DateTime CreatedUtc { get; set; }
    
    public int InstrumentTypeId { get; set; }
    public required string InstrumentTypeName { get; set; }
    public bool InstrumentTypeIsCash { get; set; }
    public bool InstrumentTypeIsFxPair { get; set; }
    
    public int AssetClassId { get; set; }
    public required string AssetClassName { get; set; }
    
    public int AssetSubclassId { get; set; }
    public required string AssetSubclassName { get; set; }
    
    public required string CurrencyId { get; set; }
}