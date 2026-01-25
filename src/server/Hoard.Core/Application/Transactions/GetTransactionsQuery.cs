using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Transactions;

public sealed class GetTransactionsQuery
    : IQuery<PagedResult<TransactionSummaryDto>>, IPagedQuery, ISortedQuery
{
    // Filters
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int? AccountId { get; init; }
    public int? InstrumentId { get; init; }
    public int? TransactionTypeId { get; init; }

    // Search
    public string? Search { get; init; }

    // Paging
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    // Sorting
    public string SortBy { get; init; } = "date";
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}

public sealed class GetTransactionsHandler(HoardContext context)
    : IQueryHandler<GetTransactionsQuery, PagedResult<TransactionSummaryDto>>
{
    public async Task<PagedResult<TransactionSummaryDto>> HandleAsync(GetTransactionsQuery query, CancellationToken ct = default)
    {
        var baseQuery = context.Transactions.AsNoTracking();

        baseQuery = ApplyFilters(baseQuery, query);
        baseQuery = ApplySearch(baseQuery, query);

        var totalCount = await baseQuery.CountAsync(ct);
        
        var sortedQuery = ApplySorting(baseQuery, query);

        var items = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToSummary())
            .ToListAsync(ct);

        return new PagedResult<TransactionSummaryDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Transaction> ApplyFilters(
        IQueryable<Transaction> query,
        GetTransactionsQuery request)
    {
        if (request.FromDate.HasValue)
            query = query.Where(t => t.Date >= request.FromDate.Value);
        
        if (request.ToDate.HasValue)
            query = query.Where(t => t.Date <= request.ToDate.Value);
        
        if (request.AccountId.HasValue)
            query = query.Where(t => t.AccountId == request.AccountId.Value);
        
        if (request.TransactionTypeId.HasValue)
            query = query.Where(t => t.TransactionTypeId == request.TransactionTypeId.Value);
        
        if (request.InstrumentId.HasValue)
            query = query.Where(t => t.InstrumentId == request.InstrumentId.Value);
        
        return query;
    }

    private static IQueryable<Transaction> ApplySearch(IQueryable<Transaction> query, GetTransactionsQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var term = request.Search.Trim();

        return query.Where(t =>
            (t.ContractNoteReference != null && t.ContractNoteReference.Contains(term)) ||
            (t.Notes != null && t.Notes.Contains(term)) ||
            (t.Instrument != null && (t.Instrument.TickerDisplay.Contains(term) || t.Instrument.Name.Contains(term))));
    }
    
    private static IOrderedQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        GetTransactionsQuery request)
    {
        var orderedQueryable = request.SortBy.ToLowerInvariant() switch
        {
            "date" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(i => i.Date)
                : query.OrderByDescending(i => i.Date),

            "instrumentname" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.Instrument != null ? t.Instrument.Name : "")
                : query.OrderByDescending(t => t.Instrument != null ? t.Instrument.Name : ""),
            
            "value" => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(i => i.Value)
                : query.OrderByDescending(i => i.Value),

            _ => query.OrderByDescending(i => i.Date)
        };

        return orderedQueryable.ThenBy(i => i.Id);
    }

    private static Expression<Func<Transaction, TransactionSummaryDto>> ProjectToSummary()
    {
        return t => new TransactionSummaryDto
        {
            Id = t.Id,
            Date = t.Date,
            ContractNoteReference = t.ContractNoteReference ?? "",
            Units = t.TransactionTypeId == TransactionType.Sell ? -t.Units! : t.Units,
            Value = t.Value,

            AccountId = t.AccountId,
            AccountName = t.Account.Name,

            InstrumentId = t.InstrumentId,
            InstrumentName = t.Instrument != null ? t.Instrument.Name : "",
            InstrumentTicker = t.Instrument != null ? t.Instrument.TickerDisplay : "",
            
            TransactionTypeId = t.TransactionTypeId,
            TransactionTypeName = t.TransactionType.Name
        };
    }
}