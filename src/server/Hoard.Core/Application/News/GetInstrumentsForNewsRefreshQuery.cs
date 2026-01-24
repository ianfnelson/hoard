using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.News;

public record GetInstrumentsForNewsRefreshQuery(int? InstrumentId) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForNewsRefreshHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForNewsRefreshQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForNewsRefreshQuery query, CancellationToken ct = default)
    {
        var instrumentsQuery = context.Instruments
            .Where(i => i.EnableNewsUpdates)
            .Where(i => i.TickerNewsUpdates != null);

        if (query.InstrumentId.HasValue)
        {
            instrumentsQuery = instrumentsQuery.Where(i => i.Id == query.InstrumentId.Value);
        }

        return await instrumentsQuery
            .Select(i => i.Id)
            .ToListAsync(ct);
    }
}
