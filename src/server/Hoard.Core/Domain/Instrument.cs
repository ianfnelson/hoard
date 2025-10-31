using System.ComponentModel.DataAnnotations;

namespace Hoard.Core.Domain;

public class Instrument : Entity<int>
{
    public required string Name { get; set; }
    
    public int InstrumentTypeId { get; set; }
    public InstrumentType InstrumentType { get; set; } = null!;
    
    public int AssetSubclassId { get; set; }
    public AssetSubclass AssetSubclass { get; set; } = null!;
    
    public required string BaseCurrencyId { get; set; }
    public Currency BaseCurrency { get; set; } = null!;
    
    public required string QuoteCurrencyId { get; set; }
    public Currency QuoteCurrency { get; set; } = null!;
    
    public string? TickerApi { get; set; }
    public required string Ticker { get; set; }
    public bool EnablePriceUpdates  { get; set; }
    public bool IsActive { get; set; } = true;
    
    public bool IsCash => InstrumentType.IsCash;
    public bool IsExternal => InstrumentType.IsExternal;
    public bool IsFxPair => InstrumentType.IsFxPair;
}