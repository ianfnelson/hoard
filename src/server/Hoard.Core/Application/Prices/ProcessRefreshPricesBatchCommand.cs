using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Services;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record ProcessRefreshPricesBatchCommand(
    Guid PricesRunId,
    PipelineMode PipelineMode,
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
            .Include(i => i.InstrumentType)
            .Include(i => i.DataFeeds)
            .FirstOrDefaultAsync(c => c.Id == command.InstrumentId, ct);

        if (instrument == null)
        {
            logger.LogWarning("Received RefreshPricesBatchCommand with unknown Instrument {InstrumentId}", command.InstrumentId);
            return;
        }

        var priceFeed = instrument.DataFeeds.FirstOrDefault(df => df.DataFeedId == DataFeed.Prices);
        if (priceFeed == null)
        {
            logger.LogWarning("Price updates not possible for Instrument {InstrumentId}", command.InstrumentId);
            return;
        }

        var prices = await priceService.GetPricesAsync(priceFeed.Configuration.Ticker, command.StartDate, command.EndDate, ct);
        var now = DateTime.UtcNow;

        var changed = new List<DateOnly>();
        await UpsertPrices(instrument.Id, prices, now, changed, ct);
        
        await context.SaveChangesAsync(ct);
        
        foreach (var date in changed)
        {
            await bus.Publish(new PriceChangedEvent(command.PricesRunId, command.PipelineMode, instrument.Id, instrument.InstrumentTypeId, date, now));
        }

        await bus.Publish(new PriceRefreshedEvent(command.PricesRunId, command.PipelineMode, instrument.Id, instrument.InstrumentTypeId, command.StartDate, command.EndDate, now));
        logger.LogInformation("Prices refreshed for Instrument {InstrumentId}", instrument.Id);
    }

    private async Task UpsertPrices(int instrumentId, IReadOnlyList<PriceDto> priceDtos, DateTime now,
        List<DateOnly> changed, CancellationToken ct = default)
    {
        foreach (var priceDto in priceDtos)
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
            }
            
            if (!price.IsLocked)
            {
                if (HasNotableChange(price, priceDto))
                {
                    changed.Add(priceDto.Date);
                }

                price.Open = priceDto.Open;
                price.High = priceDto.High;
                price.Low = priceDto.Low;
                price.Close = priceDto.Close;
                price.Volume = priceDto.Volume;
                price.AdjustedClose = priceDto.AdjustedClose;
                price.RetrievedUtc = now;
            }
        }
    }

    private static bool HasNotableChange(Price price, PriceDto dto)
    {
        return price.Close != dto.Close;
    }
}
