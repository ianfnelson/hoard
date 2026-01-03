namespace Hoard.Core.Application.Portfolios;

public class PortfolioSnapshotsDto
{
    public int PortfolioId { get; init; }
    
    public IReadOnlyList<PortfolioSnapshotDto> Snapshots { get; init; } = [];
}