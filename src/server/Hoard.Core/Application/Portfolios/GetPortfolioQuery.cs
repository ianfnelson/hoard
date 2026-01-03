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
                Performance = p.Performance == null 
                    ? null 
                    : new PortfolioPerformanceDto
                    {
                        Value = p.Performance.Value,
                        CashValue = p.Performance.CashValue,
                        PreviousValue = p.Performance.PreviousValue,
                        ValueChange = p.Performance.ValueChange,
                        UnrealisedGain = p.Performance.UnrealisedGain,
                        RealisedGain = p.Performance.RealisedGain,
                        Income = p.Performance.Income,
                        Return1D = p.Performance.Return1D,
                        Return1W = p.Performance.Return1W,
                        Return1M = p.Performance.Return1M,
                        Return3M = p.Performance.Return3M,
                        Return6M = p.Performance.Return6M,
                        Return1Y = p.Performance.Return1Y,
                        Return3Y = p.Performance.Return3Y,
                        Return5Y = p.Performance.Return5Y,
                        Return10Y = p.Performance.Return10Y,
                        ReturnYtd = p.Performance.ReturnYtd,
                        ReturnAllTime = p.Performance.ReturnAllTime,
                        AnnualisedReturn = p.Performance.AnnualisedReturn,
                        UpdatedUtc = p.Performance.UpdatedUtc
                    }
            })
            .SingleOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "Portfolio with id {PortfolioId} not found",
                query.PortfolioId);
            return dto;
        }

        dto.Performance?.CashPercentage = 100.0M * dto.Performance.CashValue / dto.Performance.Value;

        return dto;
    }
}