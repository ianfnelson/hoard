using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Cashflows;

public record ProcessBackfillCashflowCommand(Guid CorrelationId) : ICommand;

public class ProcessBackfillCashflowHandler(ILogger<ProcessBackfillCashflowHandler> logger, HoardContext context)
    : ICommandHandler<ProcessBackfillCashflowCommand>
{
    public async Task HandleAsync(ProcessBackfillCashflowCommand command, CancellationToken ct = default)
    {
        var cashflows = await DeriveCashflows(ct);

        await PersistCashflows(cashflows, ct);

        logger.LogInformation("ProcessBackfillCashflowHandler Executed");
    }

    private async Task<List<Domain.Cashflow>> DeriveCashflows(CancellationToken ct)
    {
        var transactions = await context.Transactions
            .AsNoTracking()
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken: ct);

        var cashflows = new List<Domain.Cashflow>(transactions.Count);
        
        cashflows.AddRange(transactions.Select(t => t.ToCashflow()).OfType<Domain.Cashflow>());
        return cashflows;
    }

    private async Task PersistCashflows(List<Domain.Cashflow> cashflows, CancellationToken ct)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);

        await context.Cashflows.ExecuteDeleteAsync(cancellationToken: ct);

        await context.Cashflows.AddRangeAsync(cashflows, ct);
        await context.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
    }
}