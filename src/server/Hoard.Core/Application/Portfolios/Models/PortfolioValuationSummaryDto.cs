namespace Hoard.Core.Application.Portfolios.Models;

public class PortfolioValuationSummaryDto
{
    public decimal Value { get; init; }
    public DateOnly AsOfDate { get; init; }
}