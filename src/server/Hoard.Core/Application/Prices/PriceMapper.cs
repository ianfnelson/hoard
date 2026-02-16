using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Prices;

public class PriceMapper : IMapper<PriceWriteDto, Price>
{
    public Price Map(PriceWriteDto source)
    {
        return new Price
        {
            InstrumentId = source.InstrumentId!.Value,
            AsOfDate = source.AsOfDate!.Value,
            Source = source.Source ?? "Manual",
            IsLocked = source.IsLocked ?? false,
            Open = source.Open,
            High = source.High,
            Low = source.Low,
            Close = source.Close!.Value,
            Volume = source.Volume,
            AdjustedClose = source.AdjustedClose!.Value
        };
    }

    public void Map(PriceWriteDto source, Price destination)
    {
        destination.InstrumentId = source.InstrumentId!.Value;
        destination.AsOfDate = source.AsOfDate!.Value;
        destination.Source = source.Source ?? "Manual";
        destination.IsLocked = source.IsLocked ?? false;
        destination.Open = source.Open;
        destination.High = source.High;
        destination.Low = source.Low;
        destination.Close = source.Close!.Value;
        destination.Volume = source.Volume;
        destination.AdjustedClose = source.AdjustedClose!.Value;
    }
}
