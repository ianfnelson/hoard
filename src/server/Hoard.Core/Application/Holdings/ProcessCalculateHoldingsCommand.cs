using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Holdings;

public record ProcessCalculateHoldingsCommand(Guid CorrelationId, PipelineMode PipelineMode, DateOnly? AsOfDate) : ICommand;

public class ProcessCalculateHoldingsHandler(
    IBus bus,
    ILogger<ProcessCalculateHoldingsHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculateHoldingsCommand>
{
    public async Task HandleAsync(ProcessCalculateHoldingsCommand command, CancellationToken ct = default)
    {
        var asOfDate = command.AsOfDate.OrToday();
        
        logger.LogInformation("Starting holdings calculation for {Date}", asOfDate.ToIsoDateString());

        var changedInstruments = new HashSet<int>();
        
        await CalculateHoldings(asOfDate, changedInstruments, ct);
        
        foreach (var instrument in changedInstruments)
        {
            await bus.Publish(new HoldingChangedEvent(command.CorrelationId, command.PipelineMode, asOfDate, instrument));
        }
        await bus.Publish(new HoldingsCalculatedEvent(command.CorrelationId, command.PipelineMode, asOfDate));
    }

    private async Task CalculateHoldings(DateOnly asOfDate, HashSet<int> changedInstruments, CancellationToken ct)
    {
        var accounts = await context.Accounts
            .Select(a => a.Id)
            .ToListAsync(ct);

        foreach (var accountId in accounts)
        {
            await CalculateAccountHoldings(accountId, asOfDate, changedInstruments);
        }
    }
    
    private async Task CalculateAccountHoldings(
        int accountId,
        DateOnly asOf,
        HashSet<int> changedInstrumentIds)
    {
        // 1. Load all transactions for account up to date
        var transactions = await context.Transactions
            .Where(t => t.AccountId == accountId && t.Date <= asOf)
            .AsNoTracking()
            .ToListAsync();

        // 2. Build snapshot of holdings
        var snapshot = BuildSnapshot(transactions)
            .OrderBy(s => s.InstrumentId).ToList();

        // 3. Load existing holdings
        var existing = await context.Holdings
            .Where(h => h.AccountId == accountId && h.AsOfDate == asOf)
            .OrderBy(h => h.InstrumentId)
            .ToListAsync();
        
        var existingMap = existing.ToDictionary(h => h.InstrumentId);

        // 4. Insert or update holdings
        foreach (var s in snapshot)
        {
            if (!existingMap.TryGetValue(s.InstrumentId, out var row))
            {
                // NEW
                context.Holdings.Add(new Holding
                {
                    AccountId = accountId,
                    InstrumentId = s.InstrumentId,
                    AsOfDate = asOf,
                    Units = s.Units,
                    UpdatedUtc = DateTime.UtcNow
                });

                changedInstrumentIds.Add(s.InstrumentId);
            }
            else if (row.Units != s.Units)
            {
                // CHANGED
                row.Units = s.Units;
                row.UpdatedUtc = DateTime.UtcNow;
                changedInstrumentIds.Add(s.InstrumentId);
            }
        }

        // 5. Delete obsolete holdings
        var snapshotIds = snapshot.Select(x => x.InstrumentId).ToHashSet();
        var obsolete = existing
            .Where(e => !snapshotIds.Contains(e.InstrumentId))
            .OrderBy(e => e.InstrumentId)
            .ToList();

        foreach (var dead in obsolete)
        {
            context.Holdings.Remove(dead);
            changedInstrumentIds.Add(dead.InstrumentId);
        }

        await context.SaveChangesAsync();
    }
    
    private static List<HoldingSnapshot> BuildSnapshot(List<Transaction> transactions)
    {
        var snapshots =
            transactions.Where(t => t.CategoryId is 1 or 2 or 7)
                .GroupBy(t => t.InstrumentId)
                .Select(g => new HoldingSnapshot(
                    g.Key!.Value,
                    g.Sum(t => t.CategoryId == 2 ? -t.Units!.Value : t.Units!.Value)))
                .Where(s => s.Units != decimal.Zero)
                .ToList();

        // Cash
        var cashUnits = transactions.Sum(t => t.Value);
        if (cashUnits != decimal.Zero)
        {
            snapshots.Add(new HoldingSnapshot(Instrument.Cash, cashUnits));
        }

        return snapshots;
    }

    private sealed record HoldingSnapshot(int InstrumentId, decimal Units);
}