using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioValuationQuery(int PortfolioId) : IQuery<PortfolioValuationDetailDto?>;

public class GetPortfolioValuationHandler(HoardContext context, ILogger<GetPortfolioValuationQuery> logger)
    : IQueryHandler<GetPortfolioValuationQuery, PortfolioValuationDetailDto?>
{
    public async Task<PortfolioValuationDetailDto?> HandleAsync(GetPortfolioValuationQuery query, CancellationToken ct = default)
    {
        var today = DateOnlyHelper.TodayLocal();

        var dto = await context.PortfolioValuations
            .AsNoTracking()
            .Where(pv => pv.AsOfDate <= today)
            .Where(pv => pv.PortfolioId == query.PortfolioId)
            .Select(pv => new PortfolioValuationDetailDto
            {
                AsOfDate = pv.AsOfDate,
                UpdatedUtc = pv.UpdatedUtc,
                Value = pv.Value
            })
            .OrderByDescending(pv => pv.AsOfDate)
            .FirstOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "Valuation for Portfolio with ID {PortfolioId} not found", query.PortfolioId);
        }

        return dto;
    }
}