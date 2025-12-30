namespace Hoard.Core.Application.Portfolios;

public class PortfolioValuationDetailDto
{
    public int PortfolioId { get; init; }
    public decimal Value { get; init; }
    public DateOnly AsOfDate { get; init; }
    public DateTime UpdatedUtc { get; init; }
}