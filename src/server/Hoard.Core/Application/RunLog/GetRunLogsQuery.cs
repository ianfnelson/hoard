using System.Linq.Expressions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.RunLog;

public sealed class GetRunLogsQuery : IQuery<PagedResult<RunLogDto>>, IPagedQuery, ISortedQuery
{
    public int? RunTypeId { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string SortBy { get; init; } = "startedAtUtc";
    public string SortDirection { get; init; } = "desc";
}

public sealed class GetRunLogsHandler(HoardContext context)
    : IQueryHandler<GetRunLogsQuery, PagedResult<RunLogDto>>
{
    public async Task<PagedResult<RunLogDto>> HandleAsync(GetRunLogsQuery query, CancellationToken ct = default)
    {
        var baseQuery = context.RunLogs.AsNoTracking();

        if (query.RunTypeId.HasValue)
            baseQuery = baseQuery.Where(r => r.RunTypeId == query.RunTypeId.Value);

        var totalCount = await baseQuery.CountAsync(ct);

        var sortedQuery = query.SortBy.ToLowerInvariant() switch
        {
            "runtypename" => query.SortDirection == "asc"
                ? baseQuery.OrderBy(r => r.RunType.Name)
                : baseQuery.OrderByDescending(r => r.RunType.Name),
            "completedatutc" => query.SortDirection == "asc"
                ? baseQuery.OrderBy(r => r.CompletedAtUtc)
                : baseQuery.OrderByDescending(r => r.CompletedAtUtc),
            "duration" => query.SortDirection == "asc"
                ? baseQuery.OrderBy(r => r.CompletedAtUtc.HasValue
                    ? EF.Functions.DateDiffSecond(r.StartedAtUtc, r.CompletedAtUtc.Value)
                    : (int?)null)
                : baseQuery.OrderByDescending(r => r.CompletedAtUtc.HasValue
                    ? EF.Functions.DateDiffSecond(r.StartedAtUtc, r.CompletedAtUtc.Value)
                    : (int?)null),
            _ => query.SortDirection == "asc"
                ? baseQuery.OrderBy(r => r.StartedAtUtc)
                : baseQuery.OrderByDescending(r => r.StartedAtUtc),
        };

        var items = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToDto())
            .ToListAsync(ct);

        return new PagedResult<RunLogDto> { Items = items, TotalCount = totalCount };
    }

    private static Expression<Func<Domain.Entities.RunLog, RunLogDto>> ProjectToDto() =>
        r => new RunLogDto
        {
            RunLogId = r.Id,
            RunTypeId = r.RunTypeId,
            RunTypeName = r.RunType.Name,
            RunId = r.RunId,
            StartedAtUtc = r.StartedAtUtc,
            CompletedAtUtc = r.CompletedAtUtc,
        };
}
