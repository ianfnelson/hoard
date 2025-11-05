using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Core.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class FetchPricesCommandHandler : IHandleMessages<FetchPricesCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    
    public FetchPricesCommandHandler(IBus bus, HoardContext context)
    {
        _bus = bus;
        _context = context;
    }
    
    public async Task Handle(FetchPricesCommand message)
    {
        var asOfDate = message.AsOfDate.OrToday();
        
        var instrumentIds = await GetInstrumentIdsForRefresh();

        var delay = TimeSpan.Zero;

        foreach (var instrumentId in instrumentIds)
        {
            await _bus.Defer(delay, new FetchPriceCommand(instrumentId, asOfDate));
            delay+=TimeSpan.FromSeconds(5);
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