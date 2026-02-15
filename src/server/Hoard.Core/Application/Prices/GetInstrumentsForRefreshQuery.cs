using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Prices;

public record GetInstrumentsForRefreshQuery(int? InstrumentId) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForRefreshHandler(HoardContext context, ILogger<GetInstrumentsForRefreshHandler> logger)
    : IQueryHandler<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForRefreshQuery query, CancellationToken ct = default)
    {
        if (!query.InstrumentId.HasValue)
        {
            return await context.Instruments
                .Where(i => i.DataFeeds.Any(df =>
                    df.DataFeedId == DataFeed.Prices || df.DataFeedId == DataFeed.Quotes))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken: ct);
        }
        
        var id = query.InstrumentId.Value;
        
        var exists = await context.Instruments.AnyAsync(x => x.Id == id, cancellationToken: ct);
        if (!exists)
        {
            logger.LogWarning("Instrument with id {InstrumentId} not found", id);
            return [];
        }
        
        return [id];
    }
}