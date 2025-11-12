using Hoard.Core.Application.Transactions.Models;
using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Messages.Transactions;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Transactions;

public record UpdateTransactionCommand(int TransactionId, TransactionWriteDto Dto) : ICommand;

public class UpdateTransactionHandler(
    HoardContext context,
    ITransactionFactory factory,
    IBus bus)
    : ICommandHandler<UpdateTransactionCommand>
{
    public async Task HandleAsync(UpdateTransactionCommand command, CancellationToken ct = default)
    {
        var (transactionId, dto) = command;
        
        // TODO - validate the DTO

        var tx = await GetExistingTransaction(transactionId, ct);

        UpdateTransaction(tx, dto);

        await context.SaveChangesAsync(ct);

        await bus.Publish(new TransactionUpdatedEvent(tx.Id, tx.Date));
    }

    private async Task<Transaction> GetExistingTransaction(int id, CancellationToken ct = default)
    {
        var tx = await context.Transactions
            .Include(t => t.Legs)
            .SingleOrDefaultAsync(t => t.Id == id, ct);

        return tx ?? throw new InvalidOperationException($"Transaction {id} not found.");
    }

    private void UpdateTransaction(Transaction tx, TransactionWriteDto dto)
    {
        var nexTx = factory.Create(dto);

        tx.AccountId = nexTx.AccountId;
        tx.InstrumentId = nexTx.InstrumentId;
        tx.CategoryId = nexTx.CategoryId;
        tx.ContractNoteReference = nexTx.ContractNoteReference;
        tx.Date = nexTx.Date;
        tx.Notes = nexTx.Notes;

        context.TransactionLegs.RemoveRange(tx.Legs);

        foreach (var leg in nexTx.Legs)
        {
            leg.TransactionId = tx.Id;
            tx.Legs.Add(leg);
        }
    }
}