using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Messages.Prices;
using Hoard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class BackfillPricesInstrumentCommandHandler : IHandleMessages<BackfillPricesForInstrumentCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    private readonly PriceService _priceService;
    private readonly ILogger<BackfillPricesInstrumentCommandHandler> _logger;
    
    public BackfillPricesInstrumentCommandHandler(
        IBus bus, 
        HoardContext context, 
        ILogger<BackfillPricesInstrumentCommandHandler> logger, 
        PriceService priceService)
    {
        _bus = bus;
        _context = context;
        _logger = logger;
        _priceService = priceService;
    }

    public async Task Handle(BackfillPricesForInstrumentCommand message)
    {
        var instrument = await _context.Instruments
            .FindAsync(message.InstrumentId);
        if (instrument == null)
        {
            _logger.LogWarning("Received BackfillPricesBatchCommand with unknown Instrument {InstrumentId}", message.InstrumentId);
            return;
        }

        if (instrument.TickerApi == null || !instrument.EnablePriceUpdates)
        {
            _logger.LogWarning("Price updates not possible for Instrument {InstrumentId}", message.InstrumentId);
            return;
        }
        
        var prices = await _priceService.GetPricesAsync(instrument.TickerApi!, message.StartDate, message.EndDate);
        var now = DateTime.UtcNow;
        
        foreach (var price in prices)
        {
            await UpsertPrice(instrument.Id, price, now);
        }
        
        await _context.SaveChangesAsync();
        
        await _bus.Publish(new PricesBackfilledEvent(
            message.BatchId, message.InstrumentId, message.StartDate, message.EndDate));
    }

    private async Task UpsertPrice(int instrumentId, PriceDto priceDto, DateTime now)
    {        
        var price = await _context.Prices
            .FirstOrDefaultAsync(x => x.InstrumentId == instrumentId && x.AsOfDate == priceDto.Date);

        if (price == null)
        {
            price = new Price
            {
                Source = priceDto.Source, 
                AsOfDate = priceDto.Date,
                InstrumentId = instrumentId,
            };
            _context.Add(price);
        }

        price.UpdateFrom(priceDto);
        price.RetrievedUtc = now;
    }
}