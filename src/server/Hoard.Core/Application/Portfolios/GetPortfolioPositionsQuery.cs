using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioPositionsQuery(int PortfolioId) : IQuery<PortfolioPositionsDto?>;

public class GetPortfolioPositionsHandler(HoardContext context, ILogger<GetPortfolioHandler> logger)
    : IQueryHandler<GetPortfolioPositionsQuery, PortfolioPositionsDto?>
{
    public async Task<PortfolioPositionsDto?> HandleAsync(GetPortfolioPositionsQuery query, CancellationToken ct = default)
    {
        if (!await PortfolioExists(query, ct))
        {
            logger.LogWarning(
                "Portfolio with id {PortfolioId} not found",
                query.PortfolioId);
            return null;
        }
        
        var portfolioValuation = await GetLatestPortfolioValuation(query, ct);
        
        var positions = await context.Positions
            .AsNoTracking()
            .Where(p => p.PortfolioId == query.PortfolioId)
            .Select(p => new PortfolioPositionDto
            {
                InstrumentId = p.InstrumentId,
                InstrumentName = p.Instrument.Name,
                InstrumentTicker = p.Instrument.Ticker,
                OpenDate = p.OpenDate,
                CloseDate = p.CloseDate,
                Performance = p.Performance == null
                ? null
                : new PositionPerformanceDto
                {
                    CostBasis = p.Performance.CostBasis,
                    Units = p.Performance.Units,
                    Value = p.Performance.Value,
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
            .ToListAsync(ct);

        ApplyPortfolioPercentages(positions, portfolioValuation);

        return new PortfolioPositionsDto
        {
            PortfolioId = query.PortfolioId,
            Positions = positions
        };
    }

    private Task<bool> PortfolioExists(GetPortfolioPositionsQuery query, CancellationToken ct)
    {
        return context.Portfolios
            .AnyAsync(x => x.Id == query.PortfolioId, ct);
    }

    private async Task<PortfolioValuation?> GetLatestPortfolioValuation(GetPortfolioPositionsQuery query, CancellationToken ct)
    {
        return await context.PortfolioValuations
            .AsNoTracking()
            .Where(pv => pv.PortfolioId == query.PortfolioId)
            .OrderByDescending(pv => pv.AsOfDate)
            .FirstOrDefaultAsync(ct);
    }

    private static void ApplyPortfolioPercentages(
        IEnumerable<PortfolioPositionDto> positions,
        PortfolioValuation? valuation)
    {
        if (valuation?.Value is not > 0) return;

        foreach (var position in positions)
        {
            if (position.Performance?.Value is not null)
            {
                position.PortfolioPercentage = 100.0M * position.Performance.Value / valuation.Value;
            }
        }
    }
}