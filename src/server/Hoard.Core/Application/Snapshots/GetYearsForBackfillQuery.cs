using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Snapshots;

public record GetYearsForBackfillQuery(int? StartYear, int? EndYear)
    : IQuery<IReadOnlyList<int>>;

public class GetYearsForBackfillHandler(HoardContext context)
    : IQueryHandler<GetYearsForBackfillQuery, IReadOnlyList<int>>
{
    public async Task<IReadOnlyList<int>> HandleAsync(GetYearsForBackfillQuery query, CancellationToken ct = default)
    {
        var yearRange = await GetYearRange(query, ct);

        return Enumerable
            .Range(yearRange.StartYear, yearRange.EndYear - yearRange.StartYear + 1)
            .ToList();
    }

    private async Task<YearRange> GetYearRange(GetYearsForBackfillQuery query, CancellationToken ct = default)
    {
        var startYear = query.StartYear ?? await GetDefaultStartYear(ct);
        var endYear = query.EndYear ?? DateTime.Today.Year;
        
        return new YearRange(startYear, endYear);
    }

    private async Task<int> GetDefaultStartYear(CancellationToken ct = default)
    {
        var earliestTradeDate = await context.Transactions
            .OrderBy(t => t.Date)
            .Select(x => x.Date)
            .FirstOrDefaultAsync(ct);

        return earliestTradeDate.OrToday().Year;
    }
}
