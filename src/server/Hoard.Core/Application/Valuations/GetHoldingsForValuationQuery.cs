using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Valuations;

public record GetHoldingsForValuationQuery(DateOnly AsOfDate)
    : IQuery<IReadOnlyList<int>>;

public class GetHoldingsForValuationHandler(HoardContext context)
    : IQueryHandler<GetHoldingsForValuationQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetHoldingsForValuationQuery query, CancellationToken ct = default)
    {
        return await context.Holdings
            .Where(x => x.AsOfDate == query.AsOfDate)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }
}