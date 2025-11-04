namespace Hoard.Core.Services;

public interface QuoteService
{
    Task<Dictionary<string, QuoteDto>> GetQuotesAsync(IEnumerable<string> tickers);
}

public class QuoteDto
{
    public required string Symbol { get; set; }
    public required string Name { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal FiftyTwoWeekHigh { get; set; }
    public decimal FiftyTwoWeekLow { get; set; }
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketChange { get; set; }
    public decimal RegularMarketChangePercent { get; set; }
    public required string Source { get; set; }
}
