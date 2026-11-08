using Hoard.Core.Data;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record DeletePriceCommand(int PriceId, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand;

public class DeletePriceHandler(IBus bus, HoardContext context)
    : ICommandHandler<DeletePriceCommand>
{
    public async Task HandleAsync(DeletePriceCommand command, CancellationToken ct = default)
    {
        var price = await context.Prices
            .Include(p => p.Instrument)
            .FirstOrDefaultAsync(p => p.Id == command.PriceId, ct);

        if (price == null)
        {
            throw new KeyNotFoundException($"Price with ID {command.PriceId} not found");
        }

        // Store values before deletion
        var asOfDate = price.AsOfDate;
        var instrumentId = price.InstrumentId;
        var instrumentTypeId = price.Instrument.InstrumentTypeId;
        var retrievedUtc = DateTime.UtcNow;

        context.Prices.Remove(price);
        await context.SaveChangesAsync(ct);

        await bus.Publish(new PriceChangedEvent(
            Guid.NewGuid(),
            command.PipelineMode,
            instrumentId,
            instrumentTypeId,
            asOfDate,
            retrievedUtc));
    }
}
