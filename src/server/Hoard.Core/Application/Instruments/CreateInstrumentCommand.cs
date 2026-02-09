using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Instruments;

public record CreateInstrumentCommand(InstrumentWriteDto Dto) 
    : ICommand<int>;

public class CreateTransactionHandler(IMapper mapper, HoardContext context)
    : ICommandHandler<CreateInstrumentCommand, int>
{
    public async Task<int> HandleAsync(CreateInstrumentCommand command, CancellationToken ct = default)
    {
        var instrument = mapper.Map<InstrumentWriteDto, Instrument>(command.Dto);
        
        context.Instruments.Add(instrument);

        await context.SaveChangesAsync(ct);
        
        return instrument.Id;
    }
}