using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Instruments;

public sealed class GetInstrumentsQuery
    : IQuery<PagedResult<InstrumentSummaryDto>>, IPagedQuery, ISortedQuery
{
    // Search
    public string? Search { get; init; }
    
    // Filters
    public int? InstrumentTypeId { get; init; }
    public int? AssetClassId { get; init; }
    public int? AssetSubclassId { get; init; }
    public bool? EnablePriceUpdates { get; init; }

    // Paging
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    // Sorting
    public string SortBy { get; init; } = "name";
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}

public sealed class GetInstrumentsHandler(HoardContext context)
    : IQueryHandler<GetInstrumentsQuery, PagedResult<InstrumentSummaryDto>>
{
    public async Task<PagedResult<InstrumentSummaryDto>> HandleAsync(
        GetInstrumentsQuery query,
        CancellationToken ct = default)
    {
        var baseQuery = context.Instruments.AsNoTracking();

        baseQuery = ApplyFilters(baseQuery, query);
        baseQuery = ApplySearch(baseQuery, query);

        var totalItems = await baseQuery.CountAsync(ct);

        var sortedQuery = ApplySorting(baseQuery, query);

        var items = await sortedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToSummary())
            .ToListAsync(ct);

        return new PagedResult<InstrumentSummaryDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
    }
    
    private static IQueryable<Instrument> ApplyFilters(
        IQueryable<Instrument> query,
        GetInstrumentsQuery request)
    {
        if (request.InstrumentTypeId.HasValue)
            query = query.Where(i => i.InstrumentTypeId == request.InstrumentTypeId.Value);

        if (request.AssetClassId.HasValue)
            query = query.Where(i => i.AssetSubclass.AssetClassId == request.AssetClassId.Value);

        if (request.AssetSubclassId.HasValue)
            query = query.Where(i => i.AssetSubclassId == request.AssetSubclassId.Value);

        if (request.EnablePriceUpdates.HasValue)
            query = query.Where(i => i.EnablePriceUpdates == request.EnablePriceUpdates.Value);

        return query;
    }
    
    private static IQueryable<Instrument> ApplySearch(
        IQueryable<Instrument> query,
        GetInstrumentsQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var term = request.Search.Trim();

        return query.Where(i =>
            i.Name.Contains(term) ||
            i.Ticker.Contains(term) ||
            (i.Isin != null && i.Isin.Contains(term)));
    }
    
    private static IOrderedQueryable<Instrument> ApplySorting(
        IQueryable<Instrument> query,
        GetInstrumentsQuery request)
    {
        var orderedQueryable = request.SortBy.ToLowerInvariant() switch
        {
            "name" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(i => i.Name)
                : query.OrderByDescending(i => i.Name),

            "ticker" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(i => i.Ticker)
                : query.OrderByDescending(i => i.Ticker),

            _ => query.OrderBy(i => i.Name)
        };

        return orderedQueryable.ThenBy(i => i.Id);
    }
    
    private static Expression<Func<Instrument, InstrumentSummaryDto>> ProjectToSummary()
    {
        return i => new InstrumentSummaryDto
        {
            Id = i.Id,
            Name = i.Name,
            Ticker = i.Ticker,
            TickerApi = i.TickerApi ?? "",
            Isin = i.Isin ?? "",
            EnablePriceUpdates = i.EnablePriceUpdates,
            CreatedUtc = i.CreatedUtc,

            InstrumentTypeId = i.InstrumentTypeId,
            InstrumentTypeCode = i.InstrumentType.Code,
            InstrumentTypeName = i.InstrumentType.Name,
            InstrumentTypeIsCash = i.InstrumentType.IsCash,
            InstrumentTypeIsFxPair = i.InstrumentType.IsFxPair,

            AssetClassId = i.AssetSubclass.AssetClassId,
            AssetClassCode = i.AssetSubclass.AssetClass.Code,
            AssetClassName = i.AssetSubclass.AssetClass.Name,

            AssetSubclassId = i.AssetSubclassId,
            AssetSubclassCode = i.AssetSubclass.Code,
            AssetSubclassName = i.AssetSubclass.Name,

            CurrencyId = i.CurrencyId,
        };
    }
}