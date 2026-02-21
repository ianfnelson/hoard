namespace Hoard.Core.Application.Instruments;

public class ValuationSeriesDto
{
    public List<ValuationSeriesAccountDto> Series { get; init; } = [];
}

public class ValuationSeriesAccountDto
{
    public string Name { get; init; } = null!;
    public string? Color { get; init; }
    public List<ValuationPointDto> Data { get; init; } = [];
}

public class ValuationPointDto
{
    public long X { get; init; }
    public decimal Y { get; init; }
}
