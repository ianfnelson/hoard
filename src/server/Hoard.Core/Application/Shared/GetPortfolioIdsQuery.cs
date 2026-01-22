using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Shared;

public record GetPortfolioIdsQuery(int? PortfolioId = null, bool ActiveOnly = true)
    : IQuery<IReadOnlyList<int>>;

public class GetPortfolioIdsHandler(HoardContext context, ILogger<GetPortfolioIdsHandler> logger)
    : IQueryHandler<GetPortfolioIdsQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetPortfolioIdsQuery query, CancellationToken ct = default)
    {
        if (!query.PortfolioId.HasValue)
        {
            var portfoliosQuery = context.Portfolios.AsQueryable();

            if (query.ActiveOnly)
            {
                portfoliosQuery = portfoliosQuery.Where(x => x.IsActive);
            }

            return await portfoliosQuery
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
