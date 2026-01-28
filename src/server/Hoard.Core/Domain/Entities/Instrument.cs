namespace Hoard.Core.Domain.Entities;

public partial class Instrument : Entity<int>
{
    public required string Name { get; set; }
    
    public int InstrumentTypeId { get; set; }
    public InstrumentType InstrumentType { get; set; } = null!;
    
    public int AssetSubclassId { get; set; }
    public AssetSubclass AssetSubclass { get; set; } = null!;
    
    public required string CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    
    public required string TickerDisplay { get; set; }
    public string? TickerPriceUpdates { get; set; }
    public string? TickerNewsUpdates { get; set; }
    public string? Isin { get; set; }
    public bool EnablePriceUpdates  { get; set; }
    public bool EnableNewsUpdates { get; set; }
    public DateTime? NewsImportStartUtc { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    public Quote? Quote { get; set; }

    public ICollection<NewsArticle> NewsArticles { get; set; } = [];
}