using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Services;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record DeleteTransactionCommand(int TransactionId, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand;

public class DeleteTransactionHandler(HoardContext context, IBus bus, IBlobStorageService blobService)
    : ICommandHandler<DeleteTransactionCommand>
{
    public async Task HandleAsync(DeleteTransactionCommand command, CancellationToken ct = default)
    {
        var tx = await GetExistingTransaction(command.TransactionId, ct);

        var transactionDate = tx.Date;

        // Delete contract note blob if exists
        if (!string.IsNullOrEmpty(tx.ContractNoteReference))
        {
            await blobService.DeleteContractNoteAsync(tx.ContractNoteReference, ct);
        }

        context.Transactions.Remove(tx);
        await context.SaveChangesAsync(ct);

        await bus.Publish(new TransactionDeletedEvent(command.PipelineMode, command.TransactionId, transactionDate));
    }

    private async Task<Transaction> GetExistingTransaction(int id, CancellationToken ct = default)
    {
        var tx = await context.Transactions
            .SingleOrDefaultAsync(t => t.Id == id, ct);

        return tx ?? throw new KeyNotFoundException($"Transaction {id} not found.");
    }
}