using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Snapshots;

public record GetPortfoliosForSnapshotQuery(int? PortfolioId)
    : IQuery<IReadOnlyList<int>>;

public class GetPortfoliosForSnapshotHandler(HoardContext context, ILogger<GetPortfoliosForSnapshotQuery> logger)
    : IQueryHandler<GetPortfoliosForSnapshotQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetPortfoliosForSnapshotQuery query, CancellationToken ct = default)
    {
        if (!query.PortfolioId.HasValue)
        {
            return await context.Portfolios
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);
        }

        var id = query.PortfolioId.Value;

        var exists = await context.Portfolios.AnyAsync(x => x.Id == id, ct);
        if (!exists)
        {
            logger.LogWarning("Portfolio with id {PortfolioId} not found", id);
            return [];
        }
        
        return [id];
    }
}