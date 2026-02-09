using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Instruments;

public record UpdateInstrumentCommand(int InstrumentId, InstrumentWriteDto Dto) : ICommand;

public class UpdateInstrumentHandler(HoardContext context, IMapper mapper)
    : ICommandHandler<UpdateInstrumentCommand>
{
    public async Task HandleAsync(UpdateInstrumentCommand command, CancellationToken ct = default)
    {
        var (instrumentId, dto) = command;

        var tx = await GetExistingInstrument(instrumentId, ct);

        mapper.Map(dto, tx);

        await context.SaveChangesAsync(ct);
    }

    private async Task<Instrument> GetExistingInstrument(int id, CancellationToken ct = default)
    {
        var tx = await context.Instruments
            .FindAsync(new object?[]{id}, ct);

        return tx ?? throw new InvalidOperationException($"Instrument {id} not found.");
    }
}