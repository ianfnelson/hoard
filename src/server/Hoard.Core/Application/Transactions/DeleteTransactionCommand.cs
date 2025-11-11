using Hoard.Core.Data;
using Hoard.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record DeleteTransactionCommand(int TransactionId) : ICommand;

public class DeleteTransactionHandler(HoardContext context, IBus bus) 
    : ICommandHandler<DeleteTransactionCommand>
{
    public async Task HandleAsync(DeleteTransactionCommand command, CancellationToken ct = default)
    {
        var txn = await context.Transactions
            .SingleOrDefaultAsync(t => t.Id == command.TransactionId, ct);

        if (txn == null)
            throw new KeyNotFoundException($"Transaction {command.TransactionId} not found.");

        var transactionDate = txn.Date;
        
        context.Transactions.Remove(txn);
        await context.SaveChangesAsync(ct);
        
        await bus.Publish(new TransactionDeletedEvent(command.TransactionId, transactionDate));
    }
}