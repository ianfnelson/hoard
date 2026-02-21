using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Instruments;

public sealed class GetValuationSeriesQuery : IQuery<ValuationSeriesDto?>
{
    public int InstrumentId { get; set; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public sealed class GetValuationSeriesHandler(HoardContext context, ILogger<GetValuationSeriesHandler> logger)
    : IQueryHandler<GetValuationSeriesQuery, ValuationSeriesDto?>
{
    public async Task<ValuationSeriesDto?> HandleAsync(GetValuationSeriesQuery query, CancellationToken ct = default)
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

        var holdingsQuery = context.Holdings
            .AsNoTracking()
            .Where(h => h.InstrumentId == query.InstrumentId && h.Valuation != null);

        if (query.From.HasValue)
            holdingsQuery = holdingsQuery.Where(h => h.AsOfDate >= query.From.Value);

        if (query.To.HasValue)
            holdingsQuery = holdingsQuery.Where(h => h.AsOfDate <= query.To.Value);

        var holdings = await holdingsQuery
            .Include(h => h.Account)
            .Include(h => h.Valuation)
            .OrderBy(h => h.AsOfDate)
            .ToListAsync(ct);

        var series = holdings
            .GroupBy(h => new { h.AccountId, h.Account.Name, h.Account.Colour })
            .Select(g =>
            {
                var points = g
                    .Select(h => new { h.AsOfDate, h.Valuation!.Value })
                    .OrderBy(p => p.AsOfDate)
                    .ToList();

                var data = new List<ValuationPointDto>();

                for (var i = 0; i < points.Count; i++)
                {
                    // Insert zero points at gap boundaries to drop the area to zero
                    if (i > 0 && points[i].AsOfDate.DayNumber - points[i - 1].AsOfDate.DayNumber > 7)
                    {
                        data.Add(ToPoint(points[i - 1].AsOfDate.AddDays(1), 0));
                        data.Add(ToPoint(points[i].AsOfDate.AddDays(-1), 0));
                    }

                    data.Add(ToPoint(points[i].AsOfDate, points[i].Value));
                }

                return new ValuationSeriesAccountDto
                {
                    Name = g.Key.Name,
                    Color = g.Key.Colour,
                    Data = data
                };
            })
            .ToList();

        // Normalize X coordinates so every series shares the same set of timestamps.
        // ApexCharts stacked area charts require identical X arrays for correct stacking.
        if (series.Count > 1)
        {
            var allX = series
                .SelectMany(s => s.Data.Select(p => p.X))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (var s in series)
            {
                var lookup = s.Data.ToDictionary(p => p.X, p => p.Y);
                s.Data.Clear();
                s.Data.AddRange(allX.Select(x => new ValuationPointDto
                {
                    X = x,
                    Y = lookup.GetValueOrDefault(x, 0)
                }));
            }
        }

        return new ValuationSeriesDto { Series = series };
    }

    private static ValuationPointDto ToPoint(DateOnly date, decimal value) => new()
    {
        X = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeMilliseconds(),
        Y = value
    };
}
