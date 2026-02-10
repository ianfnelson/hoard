using Hoard.Core.Data;
using Hoard.Core.Services;

namespace Hoard.Core.Application.Transactions;

public record DeleteContractNoteCommand(int TransactionId) : ICommand;

public class DeleteContractNoteHandler(
    HoardContext context,
    IBlobStorageService blobService)
    : ICommandHandler<DeleteContractNoteCommand>
{
    public async Task HandleAsync(DeleteContractNoteCommand command, CancellationToken ct = default)
    {
        var transaction = await context.Transactions.FindAsync([command.TransactionId], ct)
            ?? throw new InvalidOperationException($"Transaction {command.TransactionId} not found");

        if (string.IsNullOrEmpty(transaction.ContractNoteReference))
            return;

        await blobService.DeleteContractNoteAsync(transaction.ContractNoteReference, ct);

        transaction.ContractNoteReference = null;
        await context.SaveChangesAsync(ct);
    }
}
