using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public sealed class GetAlertsQuery
    : IQuery<PagedResult<AlertSummaryDto>>, IPagedQuery, ISortedQuery
{
    // Filters
    public int? AlertTypeId { get; init; }
    public int? InstrumentId { get; init; }
    public int? PortfolioId { get; init; }

    // Search
    public string? Search { get; init; }

    // Paging
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    // Sorting
    public string SortBy { get; init; } = "createdUtc";
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}

public sealed class GetAlertsHandler(HoardContext context)
    : IQueryHandler<GetAlertsQuery, PagedResult<AlertSummaryDto>>
{
    public async Task<PagedResult<AlertSummaryDto>> HandleAsync(GetAlertsQuery query, CancellationToken ct = default)
    {
        var baseQuery = context.Alerts.AsNoTracking();

        baseQuery = ApplyFilters(baseQuery, query);
        baseQuery = ApplySearch(baseQuery, query);

        var totalCount = await baseQuery.CountAsync(ct);

        var sortedQuery = ApplySorting(baseQuery, query);

        var items = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToSummary())
            .ToListAsync(ct);

        return new PagedResult<AlertSummaryDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Alert> ApplyFilters(IQueryable<Alert> query, GetAlertsQuery request)
    {
        if (request.AlertTypeId.HasValue)
            query = query.Where(a => a.AlertTypeId == request.AlertTypeId.Value);

        if (request.InstrumentId.HasValue)
            query = query.Where(a => a.InstrumentId == request.InstrumentId.Value);

        if (request.PortfolioId.HasValue)
            query = query.Where(a => a.PortfolioId == request.PortfolioId.Value);

        return query;
    }

    private static IQueryable<Alert> ApplySearch(IQueryable<Alert> query, GetAlertsQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var term = request.Search.Trim();

        return query.Where(a =>
            a.Title.Contains(term) ||
            a.Message.Contains(term) ||
            (a.Instrument != null && (a.Instrument.Ticker.Contains(term) || a.Instrument.Name.Contains(term))) ||
            (a.Portfolio != null && a.Portfolio.Name.Contains(term)));
    }

    private static IOrderedQueryable<Alert> ApplySorting(IQueryable<Alert> query, GetAlertsQuery request)
    {
        var orderedQueryable = request.SortBy.ToLowerInvariant() switch
        {
            "createdutc" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(a => a.CreatedUtc)
                : query.OrderByDescending(a => a.CreatedUtc),

            "triggereddate" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(a => a.TriggeredDate)
                : query.OrderByDescending(a => a.TriggeredDate),

            "alerttypename" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(a => a.AlertType.Name)
                : query.OrderByDescending(a => a.AlertType.Name),

            "title" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(a => a.Title)
                : query.OrderByDescending(a => a.Title),

            _ => query.OrderByDescending(a => a.CreatedUtc)
                .ThenByDescending(a => a.Id)
        };

        return orderedQueryable.ThenBy(a => a.Id);
    }

    private static Expression<Func<Alert, AlertSummaryDto>> ProjectToSummary()
    {
        return a => new AlertSummaryDto
        {
            Id = a.Id,
            CreatedUtc = a.CreatedUtc,
            TriggeredDate = a.TriggeredDate,
            AlertTypeId = a.AlertTypeId,
            AlertTypeName = a.AlertType.Name,
            InstrumentId = a.InstrumentId,
            InstrumentName = a.Instrument == null ? null : a.Instrument.Name,
            PortfolioId = a.PortfolioId,
            PortfolioName = a.Portfolio == null ? null : a.Portfolio.Name,
            Title = a.Title
        };
    }
}
