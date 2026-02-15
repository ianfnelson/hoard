namespace Hoard.Core.Application.Instruments;

public class InstrumentDataFeedDto
{
    public int DataFeedId { get; set; }
    public required string DataFeedName { get; set; }
    public required string Ticker { get; set; }
    public DateTime? ImportStartUtc { get; set; }
}
