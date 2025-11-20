using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Valuations;

public record GetInstrumentsForValuationQuery(DateOnly AsOfDate)
    : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForValuationHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForValuationQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForValuationQuery query, CancellationToken ct = default)
    {
        return await context.Holdings
            .Where(x => x.AsOfDate == query.AsOfDate)
            .Select(x => x.InstrumentId)
            .Distinct()
            .ToListAsync(ct);
    }
}