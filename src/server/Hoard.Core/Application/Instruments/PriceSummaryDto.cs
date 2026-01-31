namespace Hoard.Core.Application.Instruments;

public class PriceSummaryDto
{
    public int Id { get; set; }
    
    public DateOnly AsOfDate { get; set; }
    
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal Close { get; set; }
    public long? Volume { get; set; }
    public decimal AdjustedClose { get; set; }
}