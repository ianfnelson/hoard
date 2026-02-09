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
            TickerDisplay = source.TickerDisplay!
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
        destination.TickerDisplay = source.TickerDisplay!;
    }
}