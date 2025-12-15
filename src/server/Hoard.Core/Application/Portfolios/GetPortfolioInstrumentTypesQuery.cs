using Hoard.Core.Application.Portfolios.Models;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioInstrumentTypesQuery(int PortfolioId) : IQuery<List<PortfolioInstrumentTypeDto>>;

public class GetPortfolioInstrumentTypesHandler(HoardContext context)
:IQueryHandler<GetPortfolioInstrumentTypesQuery, List<PortfolioInstrumentTypeDto>>
{
    public async Task<List<PortfolioInstrumentTypeDto>> HandleAsync(GetPortfolioInstrumentTypesQuery query, CancellationToken ct = default)
    {
        var today = DateOnlyHelper.TodayLocal();
        
        var results = await context
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
        
        PopulatePercentages(results);

        return results;
    }

    private static void PopulatePercentages(List<PortfolioInstrumentTypeDto> results)
    {
        var totalValue = results.Sum(r => r.Value);

        if (totalValue <= 0) return;
        
        foreach (var row in results)
        {
            row.Percentage = decimal.Round(100.0M * row.Value / totalValue,4);
        }
    }
}