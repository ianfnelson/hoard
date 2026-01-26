using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.News;

public sealed class GetNewsArticlesQuery
    : IQuery<PagedResult<NewsArticleSummaryDto>>, IPagedQuery, ISortedQuery
{
    // Filters
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int? InstrumentId { get; init; }
    
    // Search
    public string? Search { get; init; }
    
    // Paging
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    
    // Sorting
    public string SortBy { get; init; } = "publishedUtc";
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}

public sealed class GetNewsArticlesHandler(HoardContext context)
    : IQueryHandler<GetNewsArticlesQuery, PagedResult<NewsArticleSummaryDto>>
{
    public async Task<PagedResult<NewsArticleSummaryDto>> HandleAsync(GetNewsArticlesQuery query, CancellationToken ct = default)
    {
        var baseQuery = context.NewsArticles.AsNoTracking();

        baseQuery = ApplyFilters(baseQuery, query);
        baseQuery = ApplySearch(baseQuery, query);

        var totalCount = await baseQuery.CountAsync(ct);
        
        var sortedQuery = ApplySorting(baseQuery, query);

        var items = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToSummary())
            .ToListAsync(ct);

        return new PagedResult<NewsArticleSummaryDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static IQueryable<NewsArticle> ApplyFilters(IQueryable<NewsArticle> query, GetNewsArticlesQuery request)
    {
        if (request.FromDate.HasValue)
            query = query.Where(t => t.PublishedUtc >= request.FromDate.Value);
        
        if (request.ToDate.HasValue)
            query = query.Where(t => t.PublishedUtc <= request.ToDate.Value);
        
        if (request.InstrumentId.HasValue)
            query = query.Where(t => t.InstrumentId == request.InstrumentId.Value);
        
        return query;
    }

    private IQueryable<NewsArticle> ApplySearch(IQueryable<NewsArticle> query, GetNewsArticlesQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var term = request.Search.Trim();

        return query.Where(n =>
            n.Headline.Contains(term) ||
            n.Instrument.TickerDisplay.Contains(term) ||
            n.Instrument.Name.Contains(term));
    }

    private static IOrderedQueryable<NewsArticle> ApplySorting(IQueryable<NewsArticle> query, GetNewsArticlesQuery request)
    {
        var orderedQueryable = request.SortBy.ToLowerInvariant() switch
        {
            "publishedutc" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(n => n.PublishedUtc).ThenBy(n => n.Instrument.Name)
                : query.OrderByDescending(n => n.PublishedUtc).ThenBy(n => n.Instrument.Name),
    
            "instrumentticker" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(n => n.Instrument.TickerDisplay).ThenBy(n => n.Headline)
                : query.OrderByDescending(n => n.Instrument.TickerDisplay).ThenBy(n => n.Headline),
            
            "instrumentname" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(n => n.Instrument.Name).ThenBy(n => n.Headline)
                : query.OrderByDescending(n => n.Instrument.Name).ThenBy(n => n.Headline),
            
            "headline" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(n => n.Headline).ThenBy(n => n.PublishedUtc)
                : query.OrderByDescending(n => n.Headline).ThenBy(n => n.PublishedUtc),

            _ => query.OrderByDescending(n => n.Id)
        };

        return orderedQueryable.ThenBy(i => i.Id);
    }

    private static Expression<Func<NewsArticle, NewsArticleSummaryDto>> ProjectToSummary()
    {
        return n => new NewsArticleSummaryDto
        {
            Id = n.Id,
            PublishedUtc = n.PublishedUtc,
            RetrievedUtc = n.RetrievedUtc,
            Headline = n.Headline,
            InstrumentId = n.InstrumentId,
            InstrumentName = n.Instrument.Name,
            InstrumentTicker = n.Instrument.TickerDisplay
        };
    }
}
