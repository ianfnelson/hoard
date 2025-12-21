using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfoliosQuery : IQuery<List<PortfolioSummaryDto>>;

public class GetPortfoliosHandler(HoardContext context)
: IQueryHandler<GetPortfoliosQuery, List<PortfolioSummaryDto>>
{
    public async Task<List<PortfolioSummaryDto>> HandleAsync(GetPortfoliosQuery query, CancellationToken ct = default)
    {
        var dtos = await context.Portfolios
            .AsNoTracking()
            .Select(p => new PortfolioSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                IsActive = p.IsActive,
                CreatedUtc = p.CreatedUtc,
            }).ToListAsync(ct);

        return dtos;
    }
}