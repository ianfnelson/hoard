using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Quotes;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Quotes;

public class RefreshQuotesBusHandler : IHandleMessages<RefreshQuotesBusCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    
    private const int BatchSize = 5;

    public RefreshQuotesBusHandler(IBus bus, HoardContext context)
    {
        _bus = bus;
        _context = context;
    }

    public async Task Handle(RefreshQuotesBusCommand message)
    {
        var instruments = await GetInstrumentIdsForRefresh();

        // Shuffle the instruments before batching
        instruments = instruments.Shuffle();

        var delay = TimeSpan.Zero;
        
        foreach (var batch in instruments.BatchesOf(BatchSize))
        {
            await _bus.Defer(delay, new RefreshQuotesBatchBusCommand(message.CorrelationId, batch));
            delay+=TimeSpan.FromSeconds(1);
        }
    }

    private async Task<IList<int>> GetInstrumentIdsForRefresh()
    {
        return await _context
            .Instruments
            .Where(i => i.EnablePriceUpdates)
            .Where(i => i.IsActive)
            .Where(i => i.TickerApi != null)
            .Select(x => x.Id)
            .ToListAsync();
    }
}