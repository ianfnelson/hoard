using Hoard.Core.Data;
using Hoard.Core.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record DeleteTransactionCommand(int TransactionId) : ICommand;

public class DeleteTransactionHandler : ICommandHandler<DeleteTransactionCommand>
{
    private readonly HoardContext _context;
    private readonly IBus _bus;

    public DeleteTransactionHandler(HoardContext context, IBus bus)
    {
        _context = context;
        _bus = bus;
    }

    public async Task HandleAsync(DeleteTransactionCommand command, CancellationToken ct = default)
    {
        var txn = await _context.Transactions
            .SingleOrDefaultAsync(t => t.Id == command.TransactionId, ct);

        if (txn == null)
            throw new KeyNotFoundException($"Transaction {command.TransactionId} not found.");

        var transactionDate = txn.Date;
        
        _context.Transactions.Remove(txn);
        await _context.SaveChangesAsync(ct);
        
        await _bus.Publish(new TransactionDeletedEvent(command.TransactionId, transactionDate));
    }
}