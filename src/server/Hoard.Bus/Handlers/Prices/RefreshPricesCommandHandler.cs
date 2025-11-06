using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Core.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Prices;

public class RefreshPricesCommandHandler : IHandleMessages<RefreshPricesCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    
    public RefreshPricesCommandHandler(IBus bus, HoardContext context)
    {
        _bus = bus;
        _context = context;
    }
    
    public async Task Handle(RefreshPricesCommand message)
    {
        var asOfDate = message.AsOfDate.OrToday();
        
        var instrumentIds = await GetInstrumentIdsForRefresh();

        var delay = TimeSpan.Zero;

        foreach (var instrumentId in instrumentIds)
        {
            await _bus.Defer(delay, new RefreshPricesBatchCommand(message.CorrelationId, instrumentId, asOfDate, asOfDate));
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