using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record UpdateTransactionCommand(int TransactionId, TransactionWriteDto Dto, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand;

public class UpdateTransactionHandler(
    HoardContext context,
    IMapper mapper,
    IBus bus)
    : ICommandHandler<UpdateTransactionCommand>
{
    public async Task HandleAsync(UpdateTransactionCommand command, CancellationToken ct = default)
    {
        var (transactionId, dto, pipelineMode) = command;

        var tx = await GetExistingTransaction(transactionId, ct);

        // Capture original values of fields that affect holdings calculation
        var originalAccountId = tx.AccountId;
        var originalInstrumentId = tx.InstrumentId;
        var originalTransactionTypeId = tx.TransactionTypeId;
        var originalDate = tx.Date;
        var originalUnits = tx.Units;
        var originalValue = tx.Value;

        mapper.Map(dto, tx);

        await context.SaveChangesAsync(ct);

        // Only trigger holdings recalculation if material fields changed
        var holdingsAffectingFieldsChanged =
            tx.AccountId != originalAccountId ||
            tx.InstrumentId != originalInstrumentId ||
            tx.TransactionTypeId != originalTransactionTypeId ||
            tx.Date != originalDate ||
            tx.Units != originalUnits ||
            tx.Value != originalValue;

        if (holdingsAffectingFieldsChanged)
        {
            // Use the earlier date to ensure all affected days are recalculated
            var effectiveDate = tx.Date < originalDate ? tx.Date : originalDate;
            await bus.Publish(new TransactionUpdatedEvent(pipelineMode, tx.Id, effectiveDate));
        }
    }

    private async Task<Transaction> GetExistingTransaction(int id, CancellationToken ct = default)
    {
        var tx = await context.Transactions
            .FindAsync(new object?[]{id}, ct);

        return tx ?? throw new InvalidOperationException($"Transaction {id} not found.");
    }
}