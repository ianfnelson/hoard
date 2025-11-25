using Hoard.Core.Application.Transactions.Models;
using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Transactions;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record UpdateTransactionCommand(Guid CorrelationId, int TransactionId, TransactionWriteDto Dto) : ICommand;

public class UpdateTransactionHandler(
    HoardContext context,
    IMapper mapper,
    IBus bus)
    : ICommandHandler<UpdateTransactionCommand>
{
    public async Task HandleAsync(UpdateTransactionCommand command, CancellationToken ct = default)
    {
        var (correlationId, transactionId, dto) = command;
        
        // TODO - validate the DTO

        var tx = await GetExistingTransaction(transactionId, ct);

        mapper.Map(dto, tx);

        await context.SaveChangesAsync(ct);

        await bus.Publish(new TransactionUpdatedEvent(correlationId, tx.Id, tx.Date));
    }

    private async Task<Transaction> GetExistingTransaction(int id, CancellationToken ct = default)
    {
        var tx = await context.Transactions
            .FindAsync(new object?[]{id}, ct);

        return tx ?? throw new InvalidOperationException($"Transaction {id} not found.");
    }
}