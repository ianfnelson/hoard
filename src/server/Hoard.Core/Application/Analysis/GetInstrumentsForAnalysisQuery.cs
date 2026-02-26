using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Analysis;

public record GetInstrumentsForAnalysisQuery(DateOnly AsOfDate) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForAnalysisHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsForAnalysisQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForAnalysisQuery query, CancellationToken ct = default)
    {
        var heldInstrumentIds = context.Holdings
            .Where(h => h.AsOfDate == query.AsOfDate)
            .Select(h => h.InstrumentId);

        return await context.Instruments
            .Where(i => i.DataFeeds.Any(df => df.DataFeedId == DataFeed.Prices))
            .Where(i => heldInstrumentIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToListAsync(ct);
    }
}
