using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Services;
using Hoard.Messages.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Quotes;

public record ProcessRefreshQuotesBatchCommand(IReadOnlyList<int> InstrumentIds)
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
            await bus.Publish(new QuoteChangedEvent(instrument.Id, instrument.InstrumentType.IsFxPair, now));
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
            if (!freshQuotes.TryGetValue(instrument.TickerPriceUpdates!, out var dto))
                continue;

            if (instrument.Quote == null)
            {
                instrument.Quote = new Quote { InstrumentId = instrument.Id, Source = dto.Source };
                context.Add(instrument.Quote);
            }

            var hasNotableChange = HasNotableChange(instrument.Quote!, dto);

            instrument.Quote.UpdateFrom(dto);
            instrument.Quote.RetrievedUtc = now;

            if (hasNotableChange)
            {
                changed.Add(instrument);
            }
        }
    }

    private static bool HasNotableChange(Quote quote, QuoteDto dto)
    {
        return quote.RegularMarketPrice != dto.RegularMarketPrice;
    }

    private async Task<Dictionary<string, Instrument>> GetInstrumentsToBeQuoted(ProcessRefreshQuotesBatchCommand command, CancellationToken ct = default)
    {
        var instruments = await context.Instruments
            .Include(x => x.Quote)
            .Include(x => x.InstrumentType)
            .Where(x => command.InstrumentIds.Contains(x.Id))
            .Where(x => x.EnablePriceUpdates)
            .Where(x => x.TickerPriceUpdates != null)
            .ToDictionaryAsync(x => x.TickerPriceUpdates!, ct);
        
        return instruments;
    }

}