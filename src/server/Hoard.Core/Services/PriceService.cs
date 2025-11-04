namespace Hoard.Core.Services;

public interface PriceService
{
    Task<IReadOnlyList<PriceDto>> GetPricesAsync(string ticker, DateOnly from, DateOnly to);
}

public class PriceDto
{
    public DateOnly Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public decimal AdjustedClose { get; set; }
    public required string Source { get; set; }
}