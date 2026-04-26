using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public sealed class GetPortfolioValuationSeriesQuery(int portfolioId) : IQuery<PortfolioValuationSeriesDto?>
{
    public int PortfolioId { get; } = portfolioId;
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string? DisaggregateBy { get; init; }
}

public sealed class GetPortfolioValuationSeriesHandler(
    HoardContext context,
    ILogger<GetPortfolioValuationSeriesHandler> logger,
    TimeProvider timeProvider)
    : IQueryHandler<GetPortfolioValuationSeriesQuery, PortfolioValuationSeriesDto?>
{
    public async Task<PortfolioValuationSeriesDto?> HandleAsync(
        GetPortfolioValuationSeriesQuery query,
        CancellationToken ct = default)
    {
        var portfolio = await context.Portfolios
            .AsNoTracking()
            .Include(p => p.Accounts)
            .FirstOrDefaultAsync(p => p.Id == query.PortfolioId, ct);

        if (portfolio == null)
        {
            logger.LogWarning("Portfolio with id {PortfolioId} not found", query.PortfolioId);
            return null;
        }

        var to = query.To ?? DateOnlyHelper.TodayLocal(timeProvider);
        var from = query.From ?? to.AddYears(-1);

        var accountIds = portfolio.Accounts.Select(a => a.Id).ToArray();

        var holdings = await context.Holdings
            .AsNoTracking()
            .Where(h => accountIds.Contains(h.AccountId)
                        && h.AsOfDate >= from
                        && h.AsOfDate <= to
                        && h.Valuation != null)
            .Include(h => h.Account)
            .Include(h => h.Instrument.InstrumentType)
            .Include(h => h.Instrument.AssetSubclass.AssetClass)
            .Include(h => h.Valuation)
            .OrderBy(h => h.AsOfDate)
            .ToListAsync(ct);

        var series = query.DisaggregateBy switch
        {
            "accounts" => holdings
                .GroupBy(h => new { Id = h.AccountId, h.Account.Name, h.Account.Colour })
                .Select(g => BuildGroup(g.Key.Name, g.Key.Colour, g))
                .ToList(),

            "asset-classes" => holdings
                .GroupBy(h => new
                {
                    Id = h.Instrument.AssetSubclass.AssetClassId,
                    h.Instrument.AssetSubclass.AssetClass.Name,
                    h.Instrument.AssetSubclass.AssetClass.Colour
                })
                .Select(g => BuildGroup(g.Key.Name, g.Key.Colour, g))
                .ToList(),

            "asset-subclasses" => holdings
                .GroupBy(h => new
                {
                    Id = h.Instrument.AssetSubclassId,
                    h.Instrument.AssetSubclass.Name,
                    h.Instrument.AssetSubclass.Colour
                })
                .Select(g => BuildGroup(g.Key.Name, g.Key.Colour, g))
                .ToList(),

            "instrument-types" => holdings
                .GroupBy(h => new
                {
                    Id = h.Instrument.InstrumentTypeId,
                    h.Instrument.InstrumentType.Name,
                    h.Instrument.InstrumentType.Colour
                })
                .Select(g => BuildGroup(g.Key.Name, g.Key.Colour, g))
                .ToList(),

            _ => [BuildGroup("Total", null, holdings)]
        };

        // Normalize X coordinates across series for proper stacking
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
                        .Select(x => new PortfolioValuationPointDto
                        {
                            X = x,
                            Y = lookup.GetValueOrDefault(x, 0)
                        }));
                }
            }
        }

        return new PortfolioValuationSeriesDto { Series = series };
    }

    private static PortfolioValuationSeriesGroupDto BuildGroup<T>(
        string name,
        string? colour,
        IEnumerable<T> holdings) where T : Domain.Entities.Holding
    {
        var points = holdings
            .GroupBy(h => h.AsOfDate)
            .Select(g => new { Date = g.Key, Value = g.Sum(h => h.Valuation!.Value) })
            .OrderBy(p => p.Date)
            .ToList();

        var data = new List<PortfolioValuationPointDto>();

        for (var i = 0; i < points.Count; i++)
        {
            if (i > 0 && points[i].Date.DayNumber - points[i - 1].Date.DayNumber > 7)
            {
                data.Add(ToPoint(points[i - 1].Date.AddDays(1), 0));
                data.Add(ToPoint(points[i].Date.AddDays(-1), 0));
            }

            data.Add(ToPoint(points[i].Date, points[i].Value));
        }

        return new PortfolioValuationSeriesGroupDto
        {
            Name = name,
            Color = colour,
            Data = data
        };
    }

    private static PortfolioValuationPointDto ToPoint(DateOnly date, decimal value) => new()
    {
        X = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeMilliseconds(),
        Y = value
    };
}
