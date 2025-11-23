using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Performance;

public record GetInstrumentsForBackfillQuery(int? InstrumentId) : IQuery<IReadOnlyList<int>>;

public class GetInstrumentsForBackfillHandler(HoardContext context, ILogger<GetInstrumentsForBackfillHandler> logger)
: IQueryHandler<GetInstrumentsForBackfillQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetInstrumentsForBackfillQuery query, CancellationToken ct = default)
    {
        if (!query.InstrumentId.HasValue)
        {
            return await context.Positions
                .Select(position => position.InstrumentId)
                .Distinct()
                .ToListAsync(ct);
        }
        
        var id = query.InstrumentId.Value;

        var exists = await context.Positions.AnyAsync(x => x.InstrumentId == id, cancellationToken: ct);
        if (!exists)
        {
            logger.LogWarning("No position exists for instrument with ID {id}", id);
            return [];
        }

        return [id];
    }
}