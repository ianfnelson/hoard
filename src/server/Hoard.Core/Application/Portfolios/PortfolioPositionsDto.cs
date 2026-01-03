namespace Hoard.Core.Application.Portfolios;

public class PortfolioPositionsDto
{
    public int PortfolioId { get; init; }
    
    public IReadOnlyList<PortfolioPositionDto> Positions { get; init; } = [];
}