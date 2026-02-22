namespace Hoard.Core.Application.Portfolios;

public class PortfolioValuationItemDto
{
    public int InstrumentId { get; init; }

    public string InstrumentName { get; init; } = string.Empty;

    public decimal? ToValue { get; init; }

    public decimal? ToPercentage { get; init; }

    public decimal? FromValue { get; init; }

    public decimal? FromPercentage { get; init; }

    public decimal? ChangeValue { get; init; }

    public decimal? ChangePercentage { get; init; }
}
