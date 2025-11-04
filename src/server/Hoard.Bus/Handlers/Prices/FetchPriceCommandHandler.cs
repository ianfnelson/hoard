using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Extensions;
using Hoard.Core.Messages.Prices;
using Hoard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class FetchPriceCommandHandler : IHandleMessages<FetchPriceCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    private readonly PriceService _priceService;
    private readonly ILogger<FetchPriceCommandHandler> _logger;
    
    public FetchPriceCommandHandler(
        IBus bus, 
        HoardContext context, 
        ILogger<FetchPriceCommandHandler> logger, 
        PriceService priceService)
    {
        _bus = bus;
        _context = context;
        _logger = logger;
        _priceService = priceService;
    }

    public async Task Handle(FetchPriceCommand message)
    {
        var instrument = await _context.Instruments
            .FindAsync(message.InstrumentId);
        if (instrument == null)
        {
            _logger.LogWarning("Received FetchDailyPriceCommand with unknown Instrument {InstrumentId}", message.InstrumentId);
            return;
        }

        if (instrument.TickerApi == null || !instrument.EnablePriceUpdates)
        {
            _logger.LogWarning("Price updates not possible for Instrument {InstrumentId}", message.InstrumentId);
            return;
        }
        
        var prices = await _priceService.GetPricesAsync(instrument.TickerApi!, message.AsOfDate, message.AsOfDate);
        var price = prices.FirstOrDefault(x => x.Date == message.AsOfDate);
        if (price == null)
        {
            _logger.LogWarning("No price fetched for Instrument {InstrumentId}, Date {date}", message.InstrumentId, message.AsOfDate.ToIsoDateString());
            return;
        }

        var now = DateTime.UtcNow;
        await UpsertDailyPrice(message.InstrumentId, message.AsOfDate, price, now);
        await _context.SaveChangesAsync();
        
        await _bus.Publish(new PriceUpdatedEvent(instrument.Id, message.AsOfDate, now));
        _logger.LogInformation("Prices fetched for Instrument {InstrumentId}", instrument.Id);
    }

    private async Task UpsertDailyPrice(int instrumentId, DateOnly asOfDate, PriceDto priceDto, DateTime now)
    {
        var price = await _context.Prices
            .FirstOrDefaultAsync(x => x.InstrumentId == instrumentId && x.AsOfDate == asOfDate);

        if (price == null)
        {
            price = new Price
            {
                Source = priceDto.Source, 
                AsOfDate = asOfDate,
                InstrumentId = instrumentId,
            };
            _context.Add(price);
        }

        price.UpdateFrom(priceDto);
        price.RetrievedUtc = now;
    }
}