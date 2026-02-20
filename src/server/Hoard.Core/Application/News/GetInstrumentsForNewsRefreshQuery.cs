using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.News;

public record GetInstrumentsForNewsRefreshQuery(int? InstrumentId, PipelineMode PipelineMode = PipelineMode.Backfill) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForNewsRefreshHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForNewsRefreshQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForNewsRefreshQuery query, CancellationToken ct = default)
    {
        var instrumentsQuery = context.Instruments
            .Where(i => i.DataFeeds.Any(df => df.DataFeedId == DataFeed.News));

        if (query.InstrumentId.HasValue)
        {
            instrumentsQuery = instrumentsQuery.Where(i => i.Id == query.InstrumentId.Value);
        }
        else if (query.PipelineMode is PipelineMode.DaytimeReactive or PipelineMode.CloseOfDay)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var heldInstrumentIds = context.Holdings
                .Where(h => h.AsOfDate == today)
                .Select(h => h.InstrumentId);

            instrumentsQuery = instrumentsQuery.Where(i =>
                i.InstrumentTypeId == InstrumentType.FxPair ||
                heldInstrumentIds.Contains(i.Id));
        }

        return await instrumentsQuery
            .Select(i => i.Id)
            .ToListAsync(ct);
    }
}
