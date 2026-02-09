using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Instruments;

public record DeleteInstrumentCommand(int InstrumentId)
    : ICommand;

public class DeleteInstrumentHandler(HoardContext context) 
    : ICommandHandler<DeleteInstrumentCommand>
{
    public async Task HandleAsync(DeleteInstrumentCommand command, CancellationToken ct = default)
    {
        var instrument = await GetExistingInstrument(command.InstrumentId, ct);
        
        context.Instruments.Remove(instrument);
        await context.SaveChangesAsync(ct);
    }

    private async Task<Instrument> GetExistingInstrument(int id, CancellationToken ct)
    {
        var instrument = await context.Instruments
            .SingleOrDefaultAsync(i => i.Id == id, ct);
        
        return instrument ?? throw new KeyNotFoundException($"Instrument {id} not found.");
    }
}