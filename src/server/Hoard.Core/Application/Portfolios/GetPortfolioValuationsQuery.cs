using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioValuationsQuery(int PortfolioId, DateOnly? From, DateOnly? To)
    : IQuery<List<PortfolioValuationSummaryDto>>;

public class GetPortfolioValuationsHandler(HoardContext context)
    : IQueryHandler<GetPortfolioValuationsQuery, List<PortfolioValuationSummaryDto>>
{
    public async Task<List<PortfolioValuationSummaryDto>> HandleAsync(GetPortfolioValuationsQuery query, CancellationToken ct = default)
    {
        var dbQuery = context.PortfolioValuations.AsNoTracking()
            .Where(pv => pv.PortfolioId == query.PortfolioId);

        if (query.From.HasValue)
        {
            dbQuery = dbQuery.Where(pv => pv.AsOfDate >= query.From.Value);
        }
        
        if (query.To.HasValue)
        {
            dbQuery = dbQuery.Where(pv => pv.AsOfDate <= query.To.Value);
        }
        
        var dtos = await dbQuery
            .Select(pv => new PortfolioValuationSummaryDto
            {
                AsOfDate = pv.AsOfDate,
                Value = pv.Value
            }).OrderBy(pv => pv.AsOfDate)
            .ToListAsync(ct);

        return dtos;
    }
}