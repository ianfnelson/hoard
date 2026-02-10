using Hoard.Core.Data;
using Hoard.Core.Services;

namespace Hoard.Core.Application.Transactions;

public record UploadContractNoteCommand(
    int TransactionId,
    string Reference,
    Stream FileStream) : ICommand<string>;

public class UploadContractNoteHandler(
    HoardContext context,
    IBlobStorageService blobService)
    : ICommandHandler<UploadContractNoteCommand, string>
{
    public async Task<string> HandleAsync(UploadContractNoteCommand command, CancellationToken ct = default)
    {
        var transaction = await context.Transactions.FindAsync([command.TransactionId], ct)
            ?? throw new InvalidOperationException($"Transaction {command.TransactionId} not found");

        // Delete old blob if reference is changing
        if (!string.IsNullOrEmpty(transaction.ContractNoteReference)
            && transaction.ContractNoteReference != command.Reference)
        {
            await blobService.DeleteContractNoteAsync(transaction.ContractNoteReference, ct);
        }

        var blobUri = await blobService.UploadContractNoteAsync(command.Reference, command.FileStream, ct);

        transaction.ContractNoteReference = command.Reference;
        await context.SaveChangesAsync(ct);

        return blobUri;
    }
}
