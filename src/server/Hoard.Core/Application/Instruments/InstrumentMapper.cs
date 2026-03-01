using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Instruments;

public class InstrumentMapper
: IMapper<InstrumentWriteDto, Instrument>
{
    public Instrument Map(InstrumentWriteDto source)
    {
        var instrument = new Instrument
        {
            Name = source.Name!,
            CurrencyId = source.CurrencyId!,
            Ticker = source.Ticker!
        };
        Map(source, instrument);
        return instrument;
    }

    public void Map(InstrumentWriteDto source, Instrument destination)
    {
        destination.AssetSubclassId = source.AssetSubclassId!.Value;
        destination.CurrencyId = source.CurrencyId!;
        destination.InstrumentTypeId = source.InstrumentTypeId!.Value;
        destination.Isin = source.Isin;
        destination.Name = source.Name!;
        destination.Ticker = source.Ticker!;

        // Handle data feeds
        if (source.DataFeeds != null)
        {
            // Remove feeds that are now disabled
            var toRemove = destination.DataFeeds
                .Where(df => !source.DataFeeds.Any(sdf =>
                    sdf.DataFeedId == df.DataFeedId && sdf.Enabled == true))
                .ToList();

            foreach (var df in toRemove)
            {
                destination.DataFeeds.Remove(df);
            }

            // Add or update enabled feeds
            foreach (var sdf in source.DataFeeds.Where(f => f.Enabled == true))
            {
                var existing = destination.DataFeeds
                    .FirstOrDefault(df => df.DataFeedId == sdf.DataFeedId!.Value);

                if (existing != null)
                {
                    // Update configuration
                    existing.Configuration = new DataFeedConfiguration
                    {
                        Ticker = sdf.Ticker!,
                        ImportStartUtc = sdf.ImportStartUtc
                    };
                }
                else
                {
                    // Add new feed
                    destination.DataFeeds.Add(new InstrumentDataFeed
                    {
                        DataFeedId = sdf.DataFeedId!.Value,
                        Configuration = new DataFeedConfiguration
                        {
                            Ticker = sdf.Ticker!,
                            ImportStartUtc = sdf.ImportStartUtc
                        }
                    });
                }
            }
        }
    }
}