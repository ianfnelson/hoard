using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Messages.Quotes;
using Hoard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Quotes;

public class RefreshQuotesBatchCommandHandler : IHandleMessages<RefreshQuotesBatchCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    private readonly QuoteService _quoteService;
    private readonly ILogger<RefreshQuotesBatchCommandHandler> _logger;
    
    public RefreshQuotesBatchCommandHandler(
        IBus bus, 
        HoardContext context, 
        ILogger<RefreshQuotesBatchCommandHandler> logger, 
        QuoteService quoteService)
    {
        _bus = bus;
        _context = context;
        _logger = logger;
        _quoteService = quoteService;
    }
    
    public async Task Handle(RefreshQuotesBatchCommand message)
    {
        if (message.InstrumentIds.Count == 0)
        {
            _logger.LogWarning("Received RefreshQuotesBatchCommand with no instrument IDs.");
            return;
        }

        var instruments = await GetInstrumentsToBeQuoted(message);
        if (instruments.Count == 0)
        {
            _logger.LogInformation("No instruments found to update quotes for.");
            return;
        }

        var freshQuotes = await _quoteService.GetQuotesAsync(instruments.Keys);
        var now = DateTime.UtcNow;

        var changed = new List<Instrument>();
        UpsertQuotes(instruments.Values, freshQuotes, now, changed);

        await _context.SaveChangesAsync();

        foreach (var instrument in changed)
        {
            await _bus.Publish(new QuoteRefreshedEvent(message.CorrelationId, instrument.Id, now));
            _logger.LogInformation("Quote updated for instrument {InstrumentId}", instrument.Id);
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
                _context.Add(instrument.Quote);
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

    private async Task<Dictionary<string, Instrument>> GetInstrumentsToBeQuoted(RefreshQuotesBatchCommand message)
    {
        var instruments = await _context.Instruments
            .Include(x => x.Quote)
            .Where(x => message.InstrumentIds.Contains(x.Id))
            .Where(x => x.EnablePriceUpdates)
            .Where(x => x.TickerApi != null)
            .ToDictionaryAsync(x => x.TickerApi!);
        
        return instruments;
    }
}