using Hoard.Core.Application.Transactions.Models;
using Hoard.Core.Data;
using Hoard.Messages.Transactions;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record CreateTransactionCommand(TransactionWriteDto Dto) : ICommand<int>;

public class CreateTransactionHandler(IBus bus, HoardContext context, ITransactionFactory factory)
    : ICommandHandler<CreateTransactionCommand, int>
{
    public async Task<int> HandleAsync(CreateTransactionCommand command, CancellationToken ct = default)
    {
        // TODO - input validation
        
        var tx = factory.Create(command.Dto);
        context.Transactions.Add(tx);

        await context.SaveChangesAsync(ct);

        await bus.Publish(new TransactionCreatedEvent(tx.Id, tx.Date));

        return tx.Id;
    }
}