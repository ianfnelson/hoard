using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioQuery(int PortfolioId) : IQuery<PortfolioDetailDto?>;

public class GetPortfolioHandler(HoardContext context, ILogger<GetPortfolioHandler> logger)
    : IQueryHandler<GetPortfolioQuery, PortfolioDetailDto?>
{
    public async Task<PortfolioDetailDto?> HandleAsync(GetPortfolioQuery query, CancellationToken ct = default)
    {
        var dto = await context.Portfolios
            .AsNoTracking()
            .Where(p => p.Id == query.PortfolioId)
            .Select(p => new PortfolioDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                IsActive = p.IsActive,
                CreatedUtc = p.CreatedUtc,
            })
            .SingleOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "Portfolio with id {PortfolioId} not found",
                query.PortfolioId);
        }

        return dto;
    }
}