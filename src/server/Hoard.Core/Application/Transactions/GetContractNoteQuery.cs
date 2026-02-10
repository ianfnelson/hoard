using Hoard.Core.Data;
using Hoard.Core.Services;

namespace Hoard.Core.Application.Transactions;

public record GetContractNoteQuery(int TransactionId) : IQuery<ContractNoteResult>;

public record ContractNoteResult(Stream Stream, string ContentType, string FileName);

public class GetContractNoteHandler(
    HoardContext context,
    IBlobStorageService blobService)
    : IQueryHandler<GetContractNoteQuery, ContractNoteResult>
{
    public async Task<ContractNoteResult> HandleAsync(GetContractNoteQuery query, CancellationToken ct = default)
    {
        var transaction = await context.Transactions.FindAsync([query.TransactionId], ct)
            ?? throw new InvalidOperationException($"Transaction {query.TransactionId} not found");

        if (string.IsNullOrEmpty(transaction.ContractNoteReference))
            throw new InvalidOperationException("No contract note reference found for this transaction");

        var (stream, contentType) = await blobService.DownloadContractNoteAsync(
            transaction.ContractNoteReference, ct);

        return new ContractNoteResult(stream, contentType, $"{transaction.ContractNoteReference}.pdf");
    }
}
