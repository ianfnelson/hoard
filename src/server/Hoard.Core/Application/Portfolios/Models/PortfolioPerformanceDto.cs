namespace Hoard.Core.Application.Portfolios.Models;

public class PortfolioPerformanceDto
{
    public required PortfolioCumulativePerformanceDto Cumulative { get; init; }
}

public class PortfolioCumulativePerformanceDto
{
    // Snapshot values
    public decimal Value { get; init; }
    public decimal CashValue { get; init; }
    public decimal PreviousValue { get; init; }
    public decimal ValueChange { get; init; }

    // Gains
    public decimal UnrealisedGain { get; init; }
    public decimal RealisedGain { get; init; }
    public decimal Income { get; init; }

    // Returns (nullable = not enough data yet)
    public decimal? Return1D { get; init; }
    public decimal? Return1W { get; init; }
    public decimal? Return1M { get; init; }
    public decimal? Return3M { get; init; }
    public decimal? Return6M { get; init; }
    public decimal? Return1Y { get; init; }
    public decimal? Return3Y { get; init; }
    public decimal? Return5Y { get; init; }
    public decimal? ReturnYtd { get; init; }
    public decimal? ReturnAllTime { get; init; }
    public decimal? AnnualisedReturn { get; init; }

    public DateTime UpdatedUtc { get; init; }
}