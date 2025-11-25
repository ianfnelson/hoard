using Hoard.Core.Services;

namespace Hoard.Core.Domain.Entities;

public class Quote : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public DateTime RetrievedUtc { get; set; }
    
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal FiftyTwoWeekHigh { get; set; }
    public decimal FiftyTwoWeekLow { get; set; }
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketChange { get; set; }
    public decimal RegularMarketChangePercent { get; set; }
    public required string Source { get; set; }

    public void UpdateFrom(QuoteDto dto)
    {
        Bid = dto.Bid;
        Ask = dto.Ask;
        FiftyTwoWeekHigh = dto.FiftyTwoWeekHigh;
        FiftyTwoWeekLow = dto.FiftyTwoWeekLow;
        RegularMarketChange = dto.RegularMarketChange;
        RegularMarketChangePercent = dto.RegularMarketChangePercent;
        RegularMarketPrice = dto.RegularMarketPrice;
        Source = dto.Source;
    }
}