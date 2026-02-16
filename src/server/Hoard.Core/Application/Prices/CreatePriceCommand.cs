using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record CreatePriceCommand(PriceWriteDto Dto, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand<int>;

public class CreatePriceCommandHandler(
    IBus bus,
    IMapper mapper,
    HoardContext context)
    : ICommandHandler<CreatePriceCommand, int>
{
    public async Task<int> HandleAsync(CreatePriceCommand command, CancellationToken ct = default)
    {
        var price = mapper.Map<PriceWriteDto, Price>(command.Dto);
        price.RetrievedUtc = DateTime.UtcNow;

        context.Prices.Add(price);
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

        return price.Id;
    }
}
