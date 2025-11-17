using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record ProcessRefreshPricesCommand(Guid CorrelationId, DateOnly? AsOfDate) : ICommand;

public class ProcessRefreshPricesHandler(IBus bus, HoardContext context) 
    : ICommandHandler<ProcessRefreshPricesCommand>
{
    public async Task HandleAsync(ProcessRefreshPricesCommand command, CancellationToken ct = default)
    {
        var asOfDate = command.AsOfDate.OrToday();
        
        var instrumentIds = await GetInstrumentIdsForRefresh(ct);

        var delay = TimeSpan.Zero;

        foreach (var instrumentId in instrumentIds)
        {
            await bus.Defer(delay, new RefreshPricesBatchBusCommand(command.CorrelationId, instrumentId, asOfDate, asOfDate));
            delay+=TimeSpan.FromSeconds(2);
        }
    }
    
    private async Task<IList<int>> GetInstrumentIdsForRefresh(CancellationToken ct = default)
    {
        return await context
            .Instruments
            .Where(i => i.EnablePriceUpdates)
            .Where(i => i.IsActive)
            .Where(i => i.TickerApi != null)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }
}