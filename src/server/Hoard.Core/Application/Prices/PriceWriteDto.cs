namespace Hoard.Core.Application.Prices;

public class PriceWriteDto
{
    public int? InstrumentId { get; set; }
    public DateOnly? AsOfDate { get; set; }
    public string? Source { get; set; }
    public bool? IsLocked { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Close { get; set; }
    public long? Volume { get; set; }
    public decimal? AdjustedClose { get; set; }
}
