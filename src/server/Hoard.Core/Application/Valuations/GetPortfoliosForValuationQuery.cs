using Hoard.Core.Application.Performance;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Valuations;

public record GetPortfoliosForValuationQuery : IQuery<IReadOnlyList<int>>;

public class GetPortfoliosForPerformanceHandler(HoardContext context)
    : IQueryHandler<GetPortfoliosForPerformanceQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetPortfoliosForPerformanceQuery query, CancellationToken ct = default)
    {
        return await context.Portfolios
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }
}