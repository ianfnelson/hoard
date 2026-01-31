using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Instruments;

public sealed class GetPricesQuery
    : IQuery<PagedResult<PriceSummaryDto>?>, IPagedQuery
{
    public int InstrumentId { get; set; }
    
    // Paging
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class GetPricesHandler(HoardContext context, ILogger<GetPricesHandler> logger)
    : IQueryHandler<GetPricesQuery, PagedResult<PriceSummaryDto>?>
{
    public async Task<PagedResult<PriceSummaryDto>?> HandleAsync(GetPricesQuery query, CancellationToken ct = default)
    {
        var exists = await context.Instruments
            .AnyAsync(x => x.Id == query.InstrumentId, ct);

        if (!exists)
        {
            logger.LogWarning(
                "Instrument with id {InstrumentId} not found",
                query.InstrumentId);
            return null;
        }

        var baseQuery = context.Prices.AsNoTracking()
            .Where(x => x.InstrumentId == query.InstrumentId);

        var totalCount = await baseQuery.CountAsync(ct);
        
        var sortedQuery = baseQuery.OrderByDescending(x => x.AsOfDate);

        var items = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToSummary())
            .ToListAsync(ct);

        return new PagedResult<PriceSummaryDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static Expression<Func<Price, PriceSummaryDto>> ProjectToSummary()
    {
        return p => new PriceSummaryDto
        {
            Id = p.Id,
            AsOfDate = p.AsOfDate,
            Open = p.Open,
            High = p.High,
            Low = p.Low,
            Close = p.Close,
            AdjustedClose = p.AdjustedClose,
            Volume = p.Volume
        };
    }
}