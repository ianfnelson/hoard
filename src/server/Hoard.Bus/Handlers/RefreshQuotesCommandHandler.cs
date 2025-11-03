using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Core.Messages;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers;

public class RefreshQuotesCommandHandler : IHandleMessages<RefreshQuotesCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    
    private const int BatchSize = 5;

    public RefreshQuotesCommandHandler(IBus bus, HoardContext context)
    {
        _bus = bus;
        _context = context;
    }

    public async Task Handle(RefreshQuotesCommand message)
    {
        var instruments = await GetInstrumentIdsForRefresh();

        // Shuffle the instruments before batching
        instruments = instruments.Shuffle();

        var delay = TimeSpan.Zero;
        
        foreach (var batch in instruments.BatchesOf(BatchSize))
        {
            await _bus.Defer(delay, new RefreshQuotesBatchCommand(batch));
            delay+=TimeSpan.FromSeconds(1);
        }
    }

    private async Task<IList<int>> GetInstrumentIdsForRefresh()
    {
        // We only want to refresh quotes for instruments that have active holdings
        return await _context.Holdings
            .Where(x => x.AsOfDate == DateOnly.FromDateTime(DateTime.UtcNow))
            .Where(x => x.Instrument.EnablePriceUpdates)
            .Where(x => x.Instrument.TickerApi != null)
            .Select(x => x.InstrumentId)
            .ToListAsync();
    }
}