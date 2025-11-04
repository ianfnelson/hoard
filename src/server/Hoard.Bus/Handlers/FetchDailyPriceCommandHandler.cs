using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Messages;
using Hoard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers;

public class FetchDailyPriceCommandHandler : IHandleMessages<FetchDailyPriceCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    private readonly IPriceService _priceService;
    private readonly ILogger<FetchDailyPriceCommandHandler> _logger;
    
    public FetchDailyPriceCommandHandler(
        IBus bus, 
        HoardContext context, 
        ILogger<FetchDailyPriceCommandHandler> logger, 
        IPriceService priceService)
    {
        _bus = bus;
        _context = context;
        _logger = logger;
        _priceService = priceService;
    }

    public async Task Handle(FetchDailyPriceCommand message)
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
            _logger.LogWarning("No price fetched for Instrument {InstrumentId}, Date {date}", message.InstrumentId, message.AsOfDate.ToString("yyyy-MM-dd"));
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
        
        UpdatePriceFields(price, priceDto, now);
    }

    private static void UpdatePriceFields(Price price, PriceDto priceDto, DateTime now)
    {
        price.AdjustedClose = priceDto.AdjustedClose;
        price.Close = priceDto.Close;
        price.High = priceDto.High;
        price.Low = priceDto.Low;
        price.Open = priceDto.Open;
        price.RetrievedUtc = now;
        price.Volume = priceDto.Volume;
    }
}