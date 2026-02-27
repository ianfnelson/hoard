using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Hoard.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
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

        await CreatePriceFromTransactionIfNeeded(tx, pipelineMode, ct);

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

    private async Task CreatePriceFromTransactionIfNeeded(Transaction tx, PipelineMode pipelineMode, CancellationToken ct)
    {
        // Only create price for Buy/Sell transactions with an instrument and a price
        if (tx.InstrumentId == null || tx.Price == null)
        {
            return;
        }

        if (tx.TransactionTypeId != TransactionType.Buy && tx.TransactionTypeId != TransactionType.Sell)
        {
            return;
        }

        // Check if a price already exists for this instrument and date
        var existingPrice = await context.Prices
            .FirstOrDefaultAsync(p => p.InstrumentId == tx.InstrumentId.Value && p.AsOfDate == tx.Date, ct);

        if (existingPrice != null)
        {
            return;
        }

        // Create new price from transaction
        var now = DateTime.UtcNow;
        var price = new Price
        {
            InstrumentId = tx.InstrumentId.Value,
            AsOfDate = tx.Date,
            Close = tx.Price.Value,
            AdjustedClose = tx.Price.Value,
            Source = "Trade",
            IsLocked = false,
            RetrievedUtc = now
        };

        context.Prices.Add(price);
        await context.SaveChangesAsync(ct);

        // Get the instrument type for the event
        var instrument = await context.Instruments.FindAsync([tx.InstrumentId.Value], ct);
        if (instrument != null)
        {
            await bus.Publish(new PriceChangedEvent(Guid.NewGuid(), pipelineMode, tx.InstrumentId.Value, instrument.InstrumentTypeId, tx.Date, now));
        }
    }
}