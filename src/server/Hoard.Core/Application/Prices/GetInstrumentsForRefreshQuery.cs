using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Prices;

public record GetInstrumentsForRefreshQuery(int? InstrumentId, PipelineMode PipelineMode = PipelineMode.Backfill) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForRefreshHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForRefreshQuery query, CancellationToken ct = default)
    {
        var instruments = context.Instruments
            .Where(i => i.DataFeeds.Any(df => df.DataFeedId == DataFeed.Prices));

        if (query.InstrumentId.HasValue)
        {
            instruments = instruments.Where(i => i.Id == query.InstrumentId.Value);
        }
        else if (query.PipelineMode is PipelineMode.DaytimeReactive or PipelineMode.CloseOfDay)
        {
            var today = DateOnlyHelper.TodayLocal();
            var heldInstrumentIds = context.Holdings
                .Where(h => h.AsOfDate == today)
                .Select(h => h.InstrumentId);

            instruments = instruments.Where(i =>
                i.InstrumentTypeId == InstrumentType.FxPair ||
                heldInstrumentIds.Contains(i.Id));
        }

        return await instruments
            .Select(i => i.Id)
            .ToListAsync(cancellationToken: ct);
    }
}