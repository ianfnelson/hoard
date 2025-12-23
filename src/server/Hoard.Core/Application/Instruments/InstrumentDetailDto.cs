namespace Hoard.Core.Application.Instruments;

public class InstrumentDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public required string Ticker { get; set; }
    public required string TickerApi { get; set; }
    public required string Isin { get; set; }
    
    public bool EnablePriceUpdates { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public int InstrumentTypeId { get; set; }
    public required string InstrumentTypeCode { get; set; }
    public required string InstrumentTypeName { get; set; }
    public bool InstrumentTypeIsCash { get; set; }
    public bool InstrumentTypeIsFxPair { get; set; }
    
    public int AssetClassId { get; set; }
    public required string AssetClassCode { get; set; }
    public required string AssetClassName { get; set; }
    
    public int AssetSubclassId { get; set; }
    public required string AssetSubclassCode { get; set; }
    public required string AssetSubclassName { get; set; }
    
    public required string BaseCurrencyId { get; set; }
    public required string BaseCurrencyName { get; set; }
    public required string QuoteCurrencyId { get; set; }
    public required string QuoteCurrencyName { get; set; }
}