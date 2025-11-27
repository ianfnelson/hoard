using Hoard.Core.Application.Prices;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Valuations;

public record GetInstrumentsForValuationQuery(DateOnly AsOfDate, int? InstrumentId)
    : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForValuationHandler(HoardContext context, ILogger<GetInstrumentsForRefreshHandler> logger)
    : IQueryHandler<GetInstrumentsForValuationQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForValuationQuery query, CancellationToken ct = default)
    {
        if (!query.InstrumentId.HasValue)
        {
            return await context.Holdings
                .Where(x => x.AsOfDate == query.AsOfDate)
                .Select(x => x.InstrumentId)
                .Distinct()
                .ToListAsync(ct);
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