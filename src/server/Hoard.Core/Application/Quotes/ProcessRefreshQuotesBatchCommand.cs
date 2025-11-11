using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Services;
using Hoard.Messages.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Quotes;

public record ProcessRefreshQuotesBatchCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds)
    : ICommand;

public class ProcessRefreshQuotesBatchHandler(
    IBus bus, HoardContext context, 
    ILogger<ProcessRefreshQuotesBatchHandler> logger,
    QuoteService quoteService) : ICommandHandler<ProcessRefreshQuotesBatchCommand>
{
    public async Task HandleAsync(ProcessRefreshQuotesBatchCommand command, CancellationToken ct = default)
    {
        if (command.InstrumentIds.Count == 0)
        {
            logger.LogWarning("Received RefreshQuotesBatchCommand with no instrument IDs.");
            return;
        }

        var instruments = await GetInstrumentsToBeQuoted(command, ct);
        if (instruments.Count == 0)
        {
            logger.LogInformation("No instruments found to update quotes for.");
            return;
        }

        var freshQuotes = await quoteService.GetQuotesAsync(instruments.Keys);
        var now = DateTime.UtcNow;

        var changed = new List<Instrument>();
        UpsertQuotes(instruments.Values, freshQuotes, now, changed);

        await context.SaveChangesAsync(ct);

        foreach (var instrument in changed)
        {
            await bus.Publish(new QuoteRefreshedEvent(command.CorrelationId, instrument.Id, now));
            logger.LogInformation("Quote updated for instrument {InstrumentId}", instrument.Id);
        }
    }
    
    private void UpsertQuotes(
        IEnumerable<Instrument> instruments,
        IReadOnlyDictionary<string, QuoteDto> freshQuotes,
        DateTime now,
        ICollection<Instrument> changed)
    {
        foreach (var instrument in instruments)
        {
            if (!freshQuotes.TryGetValue(instrument.TickerApi!, out var dto))
                continue;

            if (instrument.Quote == null)
            {
                instrument.Quote = new Quote { InstrumentId = instrument.Id, Source = dto.Source };
                context.Add(instrument.Quote);
            }

            var priceChanged = HasMeaningfulChange(instrument.Quote!, dto);

            instrument.Quote.UpdateFrom(dto);
            instrument.Quote.RetrievedUtc = now;

            if (priceChanged)
                changed.Add(instrument);
        }
    }

    private static bool HasMeaningfulChange(Quote quote, QuoteDto dto)
    {
        return quote.RegularMarketPrice != dto.RegularMarketPrice
               || quote.Bid != dto.Bid
               || quote.Ask != dto.Ask
               || quote.RegularMarketChange != dto.RegularMarketChange;
    }

    private async Task<Dictionary<string, Instrument>> GetInstrumentsToBeQuoted(ProcessRefreshQuotesBatchCommand command, CancellationToken ct = default)
    {
        var instruments = await context.Instruments
            .Include(x => x.Quote)
            .Where(x => command.InstrumentIds.Contains(x.Id))
            .Where(x => x.EnablePriceUpdates)
            .Where(x => x.TickerApi != null)
            .ToDictionaryAsync(x => x.TickerApi!, ct);
        
        return instruments;
    }

}