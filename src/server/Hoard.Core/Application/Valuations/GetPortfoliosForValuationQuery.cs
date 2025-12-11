using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Valuations;

public record GetPortfoliosForValuationQuery : IQuery<IReadOnlyList<int>>;

public class GetPortfoliosForValuationHandler(HoardContext context)
    : IQueryHandler<GetPortfoliosForValuationQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetPortfoliosForValuationQuery query, CancellationToken ct = default)
    {
        return await context.Portfolios
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }
}