using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioValuationsQuery(int PortfolioId, DateOnly? FromDate, DateOnly? ToDate)
    : IQuery<PortfolioValuationsDto?>;

public class GetPortfolioValuationsHandler(HoardContext context, ILogger<GetPortfolioValuationsHandler> logger, TimeProvider timeProvider)
    : IQueryHandler<GetPortfolioValuationsQuery, PortfolioValuationsDto?>
{
    private record InstrumentValuation(int InstrumentId, string InstrumentName, string InstrumentTicker, decimal Units, decimal Value);

    public async Task<PortfolioValuationsDto?> HandleAsync(
        GetPortfolioValuationsQuery query,
        CancellationToken ct = default)
    {
        if (!await PortfolioExists(query.PortfolioId, ct))
        {
            logger.LogWarning("Portfolio with id {PortfolioId} not found", query.PortfolioId);
            return null;
        }

        var toDate = query.ToDate ?? DateOnlyHelper.TodayLocal(timeProvider);
        var fromDate = query.FromDate;

        ValidateDates(fromDate, toDate);

        var toData = await GetAggregatedValuations(query.PortfolioId, toDate, ct);
        var fromData = fromDate.HasValue
            ? await GetAggregatedValuations(query.PortfolioId, fromDate.Value, ct)
            : null;

        var items = BuildItems(toData, fromData);

        return new PortfolioValuationsDto
        {
            PortfolioId = query.PortfolioId,
            ToDate = toDate,
            FromDate = fromDate,
            Items = items
        };
    }

    private void ValidateDates(DateOnly? fromDate, DateOnly toDate)
    {
        if (toDate > DateOnlyHelper.TodayLocal(timeProvider))
        {
            throw new ArgumentException("To date cannot be in the future");
        }

        if (fromDate.HasValue && fromDate.Value >= toDate)
        {
            throw new ArgumentException("From date must be before to date");
        }
    }

    private async Task<Dictionary<int, InstrumentValuation>> GetAggregatedValuations(
        int portfolioId,
        DateOnly asOfDate,
        CancellationToken ct)
    {
        var holdings = await GetHoldingsWithValuations(portfolioId, asOfDate, ct);

        return holdings
            .Where(h => h.Valuation != null)
            .GroupBy(h => h.InstrumentId)
            .ToDictionary(
                g => g.Key,
                g => new InstrumentValuation(
                    g.Key,
                    g.First().Instrument.Name,
                    g.First().Instrument.Ticker,
                    g.Sum(h => h.Units),
                    g.Sum(h => h.Valuation!.Value)));
    }

    private static List<PortfolioValuationItemDto> BuildItems(
        Dictionary<int, InstrumentValuation> toData,
        Dictionary<int, InstrumentValuation>? fromData)
    {
        var toTotal = toData.Values.Sum(v => v.Value);
        var fromTotal = fromData?.Values.Sum(v => v.Value) ?? 0;

        var allInstrumentIds = toData.Keys
            .Union(fromData?.Keys ?? Enumerable.Empty<int>());

        return allInstrumentIds
            .Select(id => BuildItem(id, toData, fromData, toTotal, fromTotal))
            .OrderBy(x => x.InstrumentName)
            .ToList();
    }

    private static PortfolioValuationItemDto BuildItem(
        int instrumentId,
        Dictionary<int, InstrumentValuation> toData,
        Dictionary<int, InstrumentValuation>? fromData,
        decimal toTotal,
        decimal fromTotal)
    {
        toData.TryGetValue(instrumentId, out var to);
        InstrumentValuation? from = null;
        fromData?.TryGetValue(instrumentId, out from);

        var instrumentName = (to ?? from)!.InstrumentName;

        decimal? toValue = to?.Value;
        decimal? toPercentage = to != null && toTotal > 0 ? 100.0M * to.Value / toTotal : null;

        decimal? fromValue = from?.Value;
        decimal? fromPercentage = from != null && fromTotal > 0 ? 100.0M * from.Value / fromTotal : null;

        decimal? changeValue = null;
        decimal? changePercentage = null;
        if (to != null && from != null)
        {
            changeValue = to.Value - from.Value;
            changePercentage = from.Value > 0
                ? 100.0M * (to.Value - from.Value) / from.Value
                : null;
        }

        return new PortfolioValuationItemDto
        {
            InstrumentId = instrumentId,
            InstrumentName = instrumentName,
            InstrumentTicker = (to ?? from)!.InstrumentTicker,
            ToUnits = to?.Units,
            ToValue = toValue,
            ToPercentage = toPercentage,
            FromUnits = from?.Units,
            FromValue = fromValue,
            FromPercentage = fromPercentage,
            ChangeValue = changeValue,
            ChangePercentage = changePercentage
        };
    }

    private Task<bool> PortfolioExists(int portfolioId, CancellationToken ct)
    {
        return context.Portfolios.AnyAsync(x => x.Id == portfolioId, ct);
    }

    private async Task<List<Holding>> GetHoldingsWithValuations(
        int portfolioId,
        DateOnly asOfDate,
        CancellationToken ct)
    {
        var portfolio = await context.Portfolios
            .AsNoTracking()
            .Include(p => p.Accounts)
            .FirstAsync(p => p.Id == portfolioId, ct);

        var accountIds = portfolio.Accounts.Select(a => a.Id).ToArray();

        return await context.Holdings
            .AsNoTracking()
            .Include(h => h.Instrument)
            .Include(h => h.Valuation)
            .Where(h => accountIds.Contains(h.AccountId) && h.AsOfDate == asOfDate)
            .ToListAsync(ct);
    }
}
