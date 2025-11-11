using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Valuations;

public record GetDatesForBackfillQuery(DateOnly? StartDate, DateOnly? EndDate)
    : IQuery<IReadOnlyList<DateOnly>>;

public class GetDatesForBackfillHandler(HoardContext context)
    : IQueryHandler<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>
{
    public async Task<IReadOnlyList<DateOnly>> HandleAsync(GetDatesForBackfillQuery query, CancellationToken ct = default)
    {
        var dateRange = await GetDateRange(query, ct);
        
        return Enumerable.Range(0, dateRange.EndDate.DayNumber - dateRange.StartDate.DayNumber + 1)
            .Select(i => dateRange.StartDate.AddDays(i))
            .ToList();
    }
    
    private async Task<DateRange> GetDateRange(GetDatesForBackfillQuery query, CancellationToken ct = default)
    {
        var startDate = query.StartDate ?? await GetDefaultStartDate(ct);
        var endDate = query.EndDate.OrToday();
        
        return new DateRange(startDate, endDate);
    }
    
    private async Task<DateOnly> GetDefaultStartDate(CancellationToken ct = default)
    {
        var earliestTradeDate = await context.Transactions
            .OrderBy(t => t.Date)
            .Select(x => x.Date)
            .FirstOrDefaultAsync(ct);
        
        return earliestTradeDate.OrToday();
    }
}