namespace Hoard.Core.Application.Instruments;

public class PriceSeriesDto
{
    public List<CandleDto> Candles { get; init; } = [];
}

public class CandleDto
{
    public long X { get; init; }
    public decimal[] Y { get; init; } = null!;
}
