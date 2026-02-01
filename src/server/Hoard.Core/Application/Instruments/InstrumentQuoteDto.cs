namespace Hoard.Core.Application.Instruments;

public class InstrumentQuoteDto
{
    public DateTime RetrievedUtc { get; set; }
    
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal FiftyTwoWeekHigh { get; set; }
    public decimal FiftyTwoWeekLow { get; set; }
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketChange { get; set; }
    public decimal RegularMarketChangePercent { get; set; }
}
