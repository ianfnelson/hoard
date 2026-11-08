using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record UpdatePriceCommand(int PriceId, PriceWriteDto Dto, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand;

public class UpdatePriceHandler(
    IBus bus,
    IMapper mapper,
    HoardContext context)
    : ICommandHandler<UpdatePriceCommand>
{
    public async Task HandleAsync(UpdatePriceCommand command, CancellationToken ct = default)
    {
        var price = await context.Prices
            .FirstOrDefaultAsync(p => p.Id == command.PriceId, ct);

        if (price == null)
        {
            throw new KeyNotFoundException($"Price with ID {command.PriceId} not found");
        }

        mapper.Map<PriceWriteDto, Price>(command.Dto, price);
        price.RetrievedUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        // Fetch instrument to get InstrumentTypeId for event
        var instrument = await context.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == price.InstrumentId, ct);

        if (instrument != null)
        {
            await bus.Publish(new PriceChangedEvent(
                Guid.NewGuid(),
                command.PipelineMode,
                price.InstrumentId,
                instrument.InstrumentTypeId,
                price.AsOfDate,
                price.RetrievedUtc));
        }
    }
}
