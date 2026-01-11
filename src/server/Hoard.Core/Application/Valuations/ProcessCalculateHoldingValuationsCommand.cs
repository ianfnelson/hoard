using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record ProcessCalculateHoldingValuationsCommand(Guid ValuationsRunId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate)
    : ICommand;

public class ProcessCalculateHoldingsValuationHandler(
    IBus bus,
    ILogger<ProcessCalculateHoldingsValuationHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculateHoldingValuationsCommand>
{
    public async Task HandleAsync(ProcessCalculateHoldingValuationsCommand command, CancellationToken ct = default)
    {
        var (valuationsRunId, pipelineMode, instrumentId, asOfDate) = command;

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

        var anyChanged = await CalculateValuations(holdings, ct);
        
        if (anyChanged)
        {
            await context.SaveChangesAsync(ct);
            await bus.Publish(new HoldingValuationsChangedEvent(valuationsRunId, pipelineMode, instrumentId, asOfDate));
        }
        
        await bus.Publish(new HoldingValuationsCalculatedEvent(valuationsRunId, pipelineMode, instrumentId, asOfDate));

        logger.LogInformation("Valuations calculated for Instrument {InstrumentId}, AsOfDate {AsOfDate}", instrumentId, asOfDate.ToIsoDateString());
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
            holding.Valuation = new HoldingValuation { HoldingId = holding.Id, Value = value};
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
        return holding.Instrument.CurrencyId switch
        {
            Currency.Gbp => 1M,
            Currency.Gbx => 100M,
            Currency.Usd => await GetLatestPriceForFxInstrument(Instrument.GbpUsd, holding.AsOfDate, ct),
            Currency.Eur => await GetLatestPriceForFxInstrument(Instrument.GbpEur, holding.AsOfDate, ct),
            Currency.Jpy => await GetLatestPriceForFxInstrument(Instrument.GbpJpy, holding.AsOfDate, ct),
            Currency.Dkk => await GetLatestPriceForFxInstrument(Instrument.GbpDkk, holding.AsOfDate, ct),
            Currency.Sek => await GetLatestPriceForFxInstrument(Instrument.GbpSek, holding.AsOfDate, ct),
            _ => throw new InvalidOperationException($"Unknown currency {holding.Instrument.CurrencyId}")
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
        // If we are valuing a holding for today before 18:00, use the quote if there is one.
        if (asOfDate == DateOnlyHelper.TodayLocal() 
            && DateTime.Now.TimeOfDay < new TimeSpan(18,0,0)
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