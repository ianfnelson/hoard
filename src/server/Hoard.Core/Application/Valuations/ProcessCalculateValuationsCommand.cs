using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Messages.Valuations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record ProcessCalculateValuationsCommand(Guid CorrelationId, int InstrumentId, DateOnly AsOfDate, bool IsBackfill = false)
    : ICommand;

public class ProcessCalculateHoldingsValuationHandler(
    IBus bus,
    ILogger<ProcessCalculateHoldingsValuationHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculateValuationsCommand>
{
    public async Task HandleAsync(ProcessCalculateValuationsCommand command, CancellationToken ct = default)
    {
        var (correlationId, instrumentId, asOfDate, isBackfill) = command;

        var holdings = await context.Holdings
            .Include(h => h.Instrument)
            .Include(h => h.Valuation)
            .Include(h => h.Instrument.Quote)
            .Where(h => h.InstrumentId == instrumentId)
            .Where(h => h.AsOfDate == asOfDate)
            .ToListAsync(ct);

        if (holdings.Count == 0)
        {
            return;
        }

        var changed = await CalculateValuations(holdings, ct);

        if (changed && !isBackfill)
        {
            await bus.Publish(new ValuationChangedEvent(correlationId, instrumentId, asOfDate));
        }
        
        await context.SaveChangesAsync(ct);
        
        await bus.Publish(new ValuationsCalculatedEvent(correlationId, instrumentId, asOfDate, isBackfill));

        logger.LogInformation("Valuations calculated for Instrument {InstrumentId}", instrumentId);
    }

    private async Task<bool> CalculateValuations(List<Holding> holdings, CancellationToken ct = default)
    {
        var anyChanged = false;
        foreach (var holding in holdings)
        {
            var value = await CalculateValuation(holding, ct);
            var changed = UpsertValuation(holding, value);
            anyChanged = anyChanged || changed;
        }
        return anyChanged;
    }
    
    private async Task<decimal> CalculateValuation(Holding holding, CancellationToken ct = default)
    {
        if (holding.InstrumentId == Instrument.Cash)
        {
            return holding.Units;
        }
        
        var price = await GetPrice(holding, ct);
        var fxRate = await GetFxRate(holding, ct);

        return Math.Round(holding.Units * price / fxRate, 2, MidpointRounding.AwayFromZero);
    }

    private bool UpsertValuation(Holding holding, decimal value)
    {
        if (holding.Valuation == null)
        {
            holding.Valuation = new Valuation { HoldingId = holding.Id, Value = value};
            context.Add(holding.Valuation);
            return true;
        }

        if (holding.Valuation.Value != value)
        {
            holding.Valuation.Value = value;
            holding.Valuation.UpdatedUtc = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    private async Task<decimal> GetFxRate(Holding holding, CancellationToken ct = default)
    {
        return holding.Instrument.QuoteCurrencyId switch
        {
            Currency.Gbp => 1M,
            Currency.Gbx => 100M,
            Currency.Usd => await GetLatestPriceForFxInstrument(Instrument.GbpUsd, holding.AsOfDate, ct),
            Currency.Eur => await GetLatestPriceForFxInstrument(Instrument.GbpEur, holding.AsOfDate, ct),
            Currency.Jpy => await GetLatestPriceForFxInstrument(Instrument.GbpJpy, holding.AsOfDate, ct),
            Currency.Dkk => await GetLatestPriceForFxInstrument(Instrument.GbpDkk, holding.AsOfDate, ct),
            Currency.Sek => await GetLatestPriceForFxInstrument(Instrument.GbpSek, holding.AsOfDate, ct),
            _ => throw new InvalidOperationException($"Unknown currency {holding.Instrument.QuoteCurrencyId}")
        };
    }

    private async Task<decimal> GetPrice(Holding holding, CancellationToken ct = default)
    {
        return await GetLatestPriceForInstrument(holding.Instrument, holding.AsOfDate, ct);
    }

    private async Task<decimal> GetLatestPriceForFxInstrument(int instrumentId, DateOnly asOfDate, CancellationToken ct = default)
    {
        var instrument = await context.Instruments
            .Include(x => x.Quote)
            .FirstOrDefaultAsync(x => x.Id == instrumentId, ct);

        return await GetLatestPriceForInstrument(instrument!, asOfDate, ct);
    }

    private async Task<decimal> GetLatestPriceForInstrument(Instrument instrument, DateOnly asOfDate, CancellationToken ct = default)
    {
        // If we are valuing a holding for today before 22;00, use the quote if there is one.
        if (asOfDate == DateOnlyHelper.TodayLocal() 
            && DateTime.Now.TimeOfDay < new TimeSpan(22,0,0)
            && instrument.Quote != null)
        {
            return instrument.Quote.RegularMarketPrice;
        }

        // No quote, or this is a valuation for an earlier day, or after 10pm. Look in price history.
        var price = await context.Prices
            .Where(x => x.InstrumentId == instrument.Id)
            .Where(x => x.AsOfDate <= asOfDate)
            .OrderByDescending(x => x.AsOfDate)
            .FirstOrDefaultAsync(ct);

        return price?.Close ?? decimal.Zero;
    }
}