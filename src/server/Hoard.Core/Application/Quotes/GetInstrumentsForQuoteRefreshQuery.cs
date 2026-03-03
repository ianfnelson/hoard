using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Quotes;

public record GetInstrumentsForQuoteRefreshQuery : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForQuoteRefreshHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForQuoteRefreshQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForQuoteRefreshQuery query, CancellationToken ct = default)
    {
        return await context.Instruments
            .Where(i => i.DataFeeds.Any(df => df.DataFeedId == DataFeed.Quotes))
            .Select(i => i.Id)
            .ToListAsync(ct);
    }
}
