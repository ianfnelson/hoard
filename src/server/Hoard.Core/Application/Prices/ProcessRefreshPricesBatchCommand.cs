using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
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
    IPriceService priceService,
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

        var changed = await UpsertPrices(instrument.Id, prices, now, ct);
        
        await context.SaveChangesAsync(ct);
        
        foreach (var date in changed)
        {
            await bus.Publish(new PriceChangedEvent(command.PricesRunId, command.PipelineMode, instrument.Id, instrument.InstrumentTypeId, date, now));
        }

        await bus.Publish(new PriceRefreshedEvent(command.PricesRunId, command.PipelineMode, instrument.Id, instrument.InstrumentTypeId, command.StartDate, command.EndDate, now));
        logger.LogInformation("Prices refreshed for Instrument {InstrumentId}", instrument.Id);
    }

    private async Task<List<DateOnly>> UpsertPrices(int instrumentId, IReadOnlyList<PriceDto> priceDtos, DateTime now,
        CancellationToken ct = default)
    {
        var changed = new List<DateOnly>();

        // Deduplicate by date, keeping first occurrence (defensive against bad data from external APIs)
        var distinctPrices = priceDtos
            .GroupBy(p => p.Date)
            .Select(g => g.First())
            .ToList();

        foreach (var priceDto in distinctPrices)
        {
            var price = await context.Prices
                .FirstOrDefaultAsync(x => x.InstrumentId == instrumentId && x.AsOfDate == priceDto.Date, ct);

            if (price == null)
            {
                InsertNewPrice(instrumentId, priceDto, now);
                changed.Add(priceDto.Date);
            }
            else if (!price.IsLocked)
            {
                if (TryUpdateExistingPrice(price, priceDto, now, instrumentId))
                {
                    changed.Add(priceDto.Date);
                }
            }
        }

        return changed;
    }

    private void InsertNewPrice(int instrumentId, PriceDto priceDto, DateTime now)
    {
        var price = new Price
        {
            Source = priceDto.Source,
            AsOfDate = priceDto.Date,
            InstrumentId = instrumentId,
            Open = priceDto.Open,
            High = priceDto.High,
            Low = priceDto.Low,
            Close = priceDto.Close,
            Volume = priceDto.Volume,
            AdjustedClose = priceDto.AdjustedClose,
            RetrievedUtc = now
        };
        context.Add(price);
    }

    private bool TryUpdateExistingPrice(Price price, PriceDto priceDto, DateTime now, int instrumentId)
    {
        if (IsSpuriousPrice(price, priceDto, instrumentId))
        {
            return false;
        }

        if (!HasAnyFieldChanged(price, priceDto))
        {
            return false;
        }

        var hasNotableChange = HasNotableChange(price, priceDto);

        price.Open = priceDto.Open;
        price.High = priceDto.High;
        price.Low = priceDto.Low;
        price.Close = priceDto.Close;
        price.Volume = priceDto.Volume;
        price.AdjustedClose = priceDto.AdjustedClose;
        price.RetrievedUtc = now;

        return hasNotableChange;
    }

    private bool IsSpuriousPrice(Price price, PriceDto priceDto, int instrumentId)
    {
        if (price.Close == 0)
        {
            return false;
        }

        var percentageChange = Math.Abs((priceDto.Close - price.Close) / price.Close);
        if (percentageChange > 0.20m)
        {
            logger.LogWarning(
                "Skipping spurious price update for Instrument {InstrumentId} on {Date}: Close changed from {OldClose} to {NewClose} (>{Threshold}%)",
                instrumentId, priceDto.Date, price.Close, priceDto.Close, 20);
            return true;
        }

        return false;
    }

    private static bool HasAnyFieldChanged(Price price, PriceDto priceDto)
    {
        return price.Open != priceDto.Open ||
               price.High != priceDto.High ||
               price.Low != priceDto.Low ||
               price.Close != priceDto.Close ||
               price.Volume != priceDto.Volume ||
               price.AdjustedClose != priceDto.AdjustedClose;
    }

    private static bool HasNotableChange(Price price, PriceDto dto)
    {
        return price.Close != dto.Close;
    }
}
