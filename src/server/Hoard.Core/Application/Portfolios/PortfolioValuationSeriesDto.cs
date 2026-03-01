namespace Hoard.Core.Application.Portfolios;

public class PortfolioValuationSeriesDto
{
    public List<PortfolioValuationSeriesGroupDto> Series { get; init; } = [];
}

public class PortfolioValuationSeriesGroupDto
{
    public string Name { get; init; } = null!;
    public string? Color { get; init; }
    public List<PortfolioValuationPointDto> Data { get; init; } = [];
}

public class PortfolioValuationPointDto
{
    public long X { get; init; }
    public decimal Y { get; init; }
}
