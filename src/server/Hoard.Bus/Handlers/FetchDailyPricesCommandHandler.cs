using Hoard.Core.Data;
using Hoard.Core.Messages;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers;

public class FetchDailyPricesCommandHandler : IHandleMessages<FetchDailyPricesCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    
    public FetchDailyPricesCommandHandler(IBus bus, HoardContext context)
    {
        _bus = bus;
        _context = context;
    }
    
    public async Task Handle(FetchDailyPricesCommand message)
    {
        var instruments = await GetInstrumentsForPricing(message.AsOfDate);

        var delay = TimeSpan.Zero;

        foreach (var instrument in instruments)
        {
            await _bus.Defer(delay, new FetchDailyPriceCommand(instrument, message.AsOfDate));
            delay+=TimeSpan.FromSeconds(5);
        }
    }

    private async Task<List<int>> GetInstrumentsForPricing(DateOnly asOfDate)
    {
        // We only want prices for instruments that have active holdings
        return await _context.Holdings
            .Where(x => x.AsOfDate == asOfDate)
            .Where(x => x.Instrument.EnablePriceUpdates)
            .Where(x => x.Instrument.TickerApi != null)
            .Select(x => x.InstrumentId)
            .Distinct()
            .ToListAsync();
    }
}