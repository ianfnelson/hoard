namespace Hoard.Core.Application.Instruments;

public class InstrumentDataFeedWriteDto
{
    public int? DataFeedId { get; set; }
    public bool? Enabled { get; set; }
    public string? Ticker { get; set; }
    public DateTime? ImportStartUtc { get; set; }
}
