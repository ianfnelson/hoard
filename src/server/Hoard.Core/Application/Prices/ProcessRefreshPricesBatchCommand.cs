using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Services;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record ProcessRefreshPricesBatchCommand(
    Guid CorrelationId,
    int InstrumentId,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand;

public class ProcessRefreshPricesBatchHandler(
    IBus bus,
    HoardContext context,
    PriceService priceService,
    ILogger<ProcessRefreshPricesBatchHandler> logger)
: ICommandHandler<ProcessRefreshPricesBatchCommand>
{
    public async Task HandleAsync(ProcessRefreshPricesBatchCommand command, CancellationToken ct = default)
    {
        var instrument = await context.Instruments
            .FindAsync(new object?[]{command.InstrumentId}, cancellationToken: ct);
        if (instrument == null)
        {
            logger.LogWarning("Received RefreshPricesBatchCommand with unknown Instrument {InstrumentId}", command.InstrumentId);
            return;
        }

        if (instrument.TickerApi == null || !instrument.EnablePriceUpdates)
        {
            logger.LogWarning("Price updates not possible for Instrument {InstrumentId}", command.InstrumentId);
            return;
        }
        
        var prices = await priceService.GetPricesAsync(instrument.TickerApi!, command.StartDate, command.EndDate, ct);
        var now = DateTime.UtcNow;
        
        foreach (var price in prices)
        {
            await UpsertPrice(instrument.Id, price, now, ct);
        }
        
        await context.SaveChangesAsync(ct);
        
        await bus.Publish(new PriceRefreshedEvent(command.CorrelationId, instrument.Id, command.StartDate, command.EndDate, now));
        logger.LogInformation("Prices refreshed for Instrument {InstrumentId}", instrument.Id);

    }
    
    private async Task UpsertPrice(int instrumentId, PriceDto priceDto, DateTime now, CancellationToken ct = default)
    {        
        var price = await context.Prices
            .FirstOrDefaultAsync(x => x.InstrumentId == instrumentId && x.AsOfDate == priceDto.Date, ct);

        if (price == null)
        {
            price = new Price
            {
                Source = priceDto.Source, 
                AsOfDate = priceDto.Date,
                InstrumentId = instrumentId,
            };
            context.Add(price);
            price.UpdateFrom(priceDto);
            price.RetrievedUtc = now;
        } else if (!price.IsLocked)
        {
            price.UpdateFrom(priceDto);
            price.RetrievedUtc = now;
        }
    }
}
