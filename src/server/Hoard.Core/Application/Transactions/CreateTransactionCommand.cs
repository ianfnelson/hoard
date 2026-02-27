using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Hoard.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record CreateTransactionCommand(TransactionWriteDto Dto, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ICommand<int>;

public class CreateTransactionHandler(IBus bus, IMapper mapper, HoardContext context)
    : ICommandHandler<CreateTransactionCommand, int>
{
    public async Task<int> HandleAsync(CreateTransactionCommand command, CancellationToken ct = default)
    {
        var tx = mapper.Map<TransactionWriteDto, Transaction>(command.Dto);

        context.Transactions.Add(tx);

        await context.SaveChangesAsync(ct);

        await CreatePriceFromTransactionIfNeeded(tx, command.PipelineMode, ct);

        await bus.Publish(new TransactionCreatedEvent(command.PipelineMode, tx.Id, tx.Date));

        return tx.Id;
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