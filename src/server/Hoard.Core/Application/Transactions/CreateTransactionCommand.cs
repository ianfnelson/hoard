using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record CreateTransactionCommand(TransactionWriteDto Dto, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand<int>;

public class CreateTransactionHandler(IBus bus, IMapper mapper, HoardContext context)
    : ICommandHandler<CreateTransactionCommand, int>
{
    public async Task<int> HandleAsync(CreateTransactionCommand command, CancellationToken ct = default)
    {
        var tx = mapper.Map<TransactionWriteDto, Transaction>(command.Dto);
        
        context.Transactions.Add(tx);

        await context.SaveChangesAsync(ct);

        await bus.Publish(new TransactionCreatedEvent(command.PipelineMode, tx.Id, tx.Date));

        return tx.Id;
    }
}