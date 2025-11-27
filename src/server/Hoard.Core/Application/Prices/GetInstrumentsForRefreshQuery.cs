using Hoard.Core.Data;
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
                .Include(x => x.InstrumentType)
                .Where(x => x.EnablePriceUpdates)
                .Where(x => x.TickerApi != null)
                .Select(x => x.Id)
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