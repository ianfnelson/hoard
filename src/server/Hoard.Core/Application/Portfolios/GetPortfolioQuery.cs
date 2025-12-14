using Hoard.Core.Application.Portfolios.Models;
using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioQuery(int PortfolioId) : IQuery<PortfolioDto?>;

public class GetPortfolioHandler(HoardContext context, ILogger<GetPortfolioHandler> logger)
    : IQueryHandler<GetPortfolioQuery, PortfolioDto?>
{
    public async Task<PortfolioDto?> HandleAsync(GetPortfolioQuery query, CancellationToken ct = default)
    {
        var dto = await context.Portfolios
            .AsNoTracking()
            .Where(p => p.Id == query.PortfolioId)
            .Select(p => new PortfolioDto
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