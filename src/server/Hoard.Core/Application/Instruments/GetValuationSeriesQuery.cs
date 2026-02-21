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

        // Normalize X coordinates so every series shares the same timestamps within
        // each contiguous time segment. Segments are separated by >7-day gaps in the
        // combined timeline, preventing diagonal ramps across periods where no account
        // holds the instrument, while ensuring correct stacking within overlapping periods.
        if (series.Count > 1)
        {
            var allX = series
                .SelectMany(s => s.Data.Select(p => p.X))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (allX.Count > 0)
            {
                const long sevenDays = 7 * 86_400_000L;
                var segments = new List<List<long>> { new() { allX[0] } };
                for (var i = 1; i < allX.Count; i++)
                {
                    if (allX[i] - allX[i - 1] > sevenDays)
                        segments.Add(new List<long>());
                    segments[^1].Add(allX[i]);
                }

                foreach (var s in series)
                {
                    if (s.Data.Count == 0) continue;

                    var lookup = s.Data.ToDictionary(p => p.X, p => p.Y);

                    s.Data.Clear();
                    s.Data.AddRange(segments
                        .Where(seg => seg.Any(x =>
                            lookup.TryGetValue(x, out var y) && y != 0))
                        .SelectMany(seg => seg)
                        .Select(x => new ValuationPointDto
                        {
                            X = x,
                            Y = lookup.GetValueOrDefault(x, 0)
                        }));
                }
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
