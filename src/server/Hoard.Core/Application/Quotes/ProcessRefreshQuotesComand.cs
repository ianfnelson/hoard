using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Quotes;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Quotes;

public record ProcessRefreshQuotesCommand(Guid CorrelationId) : ICommand;

public class ProcessRefreshQuotesHandler(
    IBus bus, HoardContext context)
    : ICommandHandler<ProcessRefreshQuotesCommand>
{
    private const int BatchSize = 5;
    
    public async Task HandleAsync(ProcessRefreshQuotesCommand command, CancellationToken ct = default)
    {
        var instruments = await GetInstrumentIdsForRefresh();

        // Shuffle the instruments before batching
        instruments = instruments.Shuffle();

        var delay = TimeSpan.Zero;
        
        foreach (var batch in instruments.BatchesOf(BatchSize))
        {
            await bus.Defer(delay, new RefreshQuotesBatchBusCommand(command.CorrelationId, batch));
            delay+=TimeSpan.FromSeconds(1);
        }
    }
    
    private async Task<IList<int>> GetInstrumentIdsForRefresh()
    {
        return await context
            .Instruments
            .Where(i => i.EnablePriceUpdates)
            .Where(i => i.IsActive)
            .Where(i => i.TickerApi != null)
            .Select(x => x.Id)
            .ToListAsync();
    }
}