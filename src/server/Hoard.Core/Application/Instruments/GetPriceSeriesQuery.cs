using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Instruments;

public sealed class GetPriceSeriesQuery(int instrumentId) : IQuery<PriceSeriesDto?>
{
    public int InstrumentId { get; } = instrumentId;
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public sealed class GetPriceSeriesHandler(HoardContext context, ILogger<GetPriceSeriesHandler> logger)
    : IQueryHandler<GetPriceSeriesQuery, PriceSeriesDto?>
{
    public async Task<PriceSeriesDto?> HandleAsync(GetPriceSeriesQuery query, CancellationToken ct = default)
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

        var to = query.To ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.From ?? to.AddYears(-1);

        var prices = await context.Prices
            .AsNoTracking()
            .Where(p => p.InstrumentId == query.InstrumentId
                        && p.AsOfDate >= from
                        && p.AsOfDate <= to
                        && p.Open != null
                        && p.High != null
                        && p.Low != null)
            .OrderBy(p => p.AsOfDate)
            .Select(p => new
            {
                p.AsOfDate,
                Open = p.Open!.Value,
                High = p.High!.Value,
                Low = p.Low!.Value,
                p.Close
            })
            .ToListAsync(ct);

        var candles = prices.Select(p => new CandleDto
        {
            X = new DateTimeOffset(p.AsOfDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                .ToUnixTimeMilliseconds(),
            Y = [p.Open, p.High, p.Low, p.Close]
        }).ToList();

        return new PriceSeriesDto { Candles = candles };
    }
}
