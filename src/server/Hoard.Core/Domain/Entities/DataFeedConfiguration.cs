namespace Hoard.Core.Domain.Entities;

public class DataFeedConfiguration
{
    public required string Ticker { get; set; }
    public DateTime? ImportStartUtc { get; set; }
}
