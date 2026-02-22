namespace Hoard.Core.Application.Portfolios;

public class PortfolioValuationsDto
{
    public int PortfolioId { get; init; }

    public DateOnly ToDate { get; init; }

    public DateOnly? FromDate { get; init; }

    public IReadOnlyList<PortfolioValuationItemDto> Items { get; init; } = [];
}
