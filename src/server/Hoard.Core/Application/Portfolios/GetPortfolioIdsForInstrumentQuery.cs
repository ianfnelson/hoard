using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioIdsForInstrumentQuery(int InstrumentId) : IQuery<IReadOnlyList<int>>;

public class GetPortfolioIdsForInstrumentHandler(HoardContext context)
    : IQueryHandler<GetPortfolioIdsForInstrumentQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetPortfolioIdsForInstrumentQuery query, CancellationToken ct = default)
    {
        return await context.Portfolios
            .Where(p => p.IsActive && p.Accounts.Any(a =>
                a.IsActive && context.Holdings.Any(h =>
                    h.AccountId == a.Id && h.InstrumentId == query.InstrumentId && h.Units != 0)))
            .Select(p => p.Id)
            .ToListAsync(ct);
    }
}
