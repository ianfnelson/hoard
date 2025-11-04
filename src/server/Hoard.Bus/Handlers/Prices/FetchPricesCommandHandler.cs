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
        var asOfDate = message.AsOfDate.OrTodayIfNull();
        
        var instruments = await GetInstrumentsForPricing(asOfDate);

        var delay = TimeSpan.Zero;

        foreach (var instrument in instruments)
        {
            await _bus.Defer(delay, new FetchPriceCommand(instrument, asOfDate));
            delay+=TimeSpan.FromSeconds(5);
        }
    }

    private async Task<List<int>> GetInstrumentsForPricing(DateOnly asOfDate)
    {
        return await _context
            .GetRefreshableInstrumentsAsOf(asOfDate)
            .Select(x => x.Id)
            .ToListAsync();
    }
}