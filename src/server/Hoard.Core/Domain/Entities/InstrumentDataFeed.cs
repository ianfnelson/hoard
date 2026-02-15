namespace Hoard.Core.Domain.Entities;

public class InstrumentDataFeed : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;

    public int DataFeedId { get; set; }
    public DataFeed DataFeed { get; set; } = null!;

    public DataFeedConfiguration Configuration { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }
}
