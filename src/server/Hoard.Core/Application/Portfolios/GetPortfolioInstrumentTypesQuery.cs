using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioInstrumentTypesQuery(int PortfolioId) : IQuery<PortfolioInstrumentTypesDto?>;

public class GetPortfolioInstrumentTypesHandler(HoardContext context, ILogger<GetPortfolioInstrumentTypesHandler> logger)
:IQueryHandler<GetPortfolioInstrumentTypesQuery, PortfolioInstrumentTypesDto?>
{
    public async Task<PortfolioInstrumentTypesDto?> HandleAsync(GetPortfolioInstrumentTypesQuery query, CancellationToken ct = default)
    {
        var exists = await context.Portfolios
            .AnyAsync(x => x.Id == query.PortfolioId, ct);

        if (!exists)
        {
            logger.LogWarning(
                "Portfolio with id {PortfolioId} not found",
                query.PortfolioId);
            return null;
        }
        
        var today = DateOnlyHelper.TodayLocal();
        
        var instrumentTypes = await context
            .HoldingValuations
            .AsNoTracking()
            .Where(hv =>
                hv.Holding.Account.Portfolios.Any(p => p.Id == query.PortfolioId))
            .Where(hv => hv.Holding.AsOfDate == today)
            .GroupBy(hv => hv.Holding.Instrument.InstrumentType)
            .Select(g => new PortfolioInstrumentTypeDto
            {
                Id = g.Key.Id,
                Code = g.Key.Code,
                Name = g.Key.Name,
                Value = g.Sum(hv => hv.Value)
            })
            .OrderByDescending(g => g.Value)
            .ToListAsync(ct);
        
        PopulatePercentages(instrumentTypes);

        return new PortfolioInstrumentTypesDto
        {
            InstrumentTypes = instrumentTypes,
            PortfolioId = query.PortfolioId,
            AsOfDate = today,
            TotalValue = instrumentTypes.Sum(i => i.Value)
        };
    }

    private static void PopulatePercentages(List<PortfolioInstrumentTypeDto> results)
    {
        var totalValue = results.Sum(r => r.Value);

        if (totalValue <= 0) return;
        
        foreach (var row in results)
        {
            row.Percentage = 100.0M * row.Value / totalValue;
        }
    }
}