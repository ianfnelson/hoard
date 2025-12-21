namespace Hoard.Core.Application.Portfolios.Models;

public class PortfolioValuationDetailDto
{
    public decimal Value { get; init; }
    public DateOnly AsOfDate { get; init; }
    public DateTime UpdatedUtc { get; init; }
}