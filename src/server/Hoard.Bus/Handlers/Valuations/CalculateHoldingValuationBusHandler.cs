using Hoard.Core;
using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Messages.Valuations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateHoldingValuationBusHandler : IHandleMessages<CalculateHoldingValuationBusCommand>
{
    private readonly IBus _bus;
    private readonly ILogger<CalculateHoldingValuationBusHandler> _logger;
    private readonly HoardContext _context;

    public CalculateHoldingValuationBusHandler(
        IBus bus,
        ILogger<CalculateHoldingValuationBusHandler> logger,
        HoardContext context)
    {
        _bus = bus;
        _logger = logger;
        _context = context;
    }

    public async Task Handle(CalculateHoldingValuationBusCommand message)
    {
        var (correlationId, holdingId) = message;

        var holding = await _context.Holdings
            .Include(x => x.Instrument)
            .Include(x => x.Valuation)
            .Include(x => x.Instrument.Quote)
            .Where(x => x.Id == holdingId)
            .FirstOrDefaultAsync();

        if (holding == null)
        {
            _logger.LogWarning("Received CalculateHoldingValuationCommand with unknown Holding {HoldingId}", holdingId);
            return;
        }

        var valuationGbp = await CalculateValuation(holding);

        UpsertValuation(holding, valuationGbp);
        await _context.SaveChangesAsync();

        await _bus.Publish(new HoldingValuationCalculatedEvent(correlationId, holdingId, holding.AsOfDate));
        _logger.LogInformation("Valuation calculated for Holding {HoldingId}", holdingId);
    }

    private async Task<decimal> CalculateValuation(Holding holding)
    {
        if (holding.InstrumentId == Instrument.CashGbpId)
        {
            return holding.Units;
        }
        
        var price = await GetPrice(holding);
        var fxRate = await GetFxRate(holding);

        return Math.Round(holding.Units * price / fxRate, 2, MidpointRounding.AwayFromZero);
    }

    private void UpsertValuation(Holding holding, decimal valuationGbp)
    {
        if (holding.Valuation == null)
        {
            holding.Valuation = new HoldingValuation { HoldingId = holding.Id };
            _context.Add(holding.Valuation);
        }

        holding.Valuation.ValuationGbp = valuationGbp;
        holding.Valuation.UpdatedUtc = DateTime.UtcNow;
    }

    private async Task<decimal> GetFxRate(Holding holding)
    {
        return holding.Instrument.QuoteCurrencyId switch
        {
            Currency.Gbp => 1M,
            Currency.Gbx => 100M,
            Currency.Usd => await GetLatestPriceForFxInstrument(Instrument.GbpUsdId, holding.AsOfDate),
            Currency.Eur => await GetLatestPriceForFxInstrument(Instrument.GbpEurId, holding.AsOfDate),
            Currency.Jpy => await GetLatestPriceForFxInstrument(Instrument.GbpJpyId, holding.AsOfDate),
            _ => throw new InvalidOperationException($"Unknown currency {holding.Instrument.QuoteCurrencyId}")
        };
    }

    private async Task<decimal> GetPrice(Holding holding)
    {
        return await GetLatestPriceForInstrument(holding.Instrument, holding.AsOfDate);
    }

    private async Task<decimal> GetLatestPriceForFxInstrument(int instrumentId, DateOnly asOfDate)
    {
        var instrument = await _context.Instruments
            .Include(x => x.Quote)
            .FirstOrDefaultAsync(x => x.Id == instrumentId);

        return await GetLatestPriceForInstrument(instrument!, asOfDate);
    }

    private async Task<decimal> GetLatestPriceForInstrument(Instrument instrument, DateOnly asOfDate)
    {
        // If we are valuing a holding for today, use the quote if there is one.
        if (asOfDate == DateOnlyHelper.TodayLocal() && instrument.Quote != null)
        {
            return instrument.Quote.RegularMarketPrice;
        }

        // No quote, or this is a valuation for an earlier day. Look in price history.
        var price = await _context.Prices
            .Where(x => x.InstrumentId == instrument.Id)
            .Where(x => x.AsOfDate <= asOfDate)
            .OrderByDescending(x => x.AsOfDate)
            .FirstOrDefaultAsync();

        return price?.AdjustedClose ?? decimal.Zero;
    }
}
