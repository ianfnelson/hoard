using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record ProcessCalculatePortfolioValuationCommand(Guid ValuationsRunId, PipelineMode PipelineMode, int PortfolioId, DateOnly AsOfDate)
: ICommand;

public class ProcessCalculatePortfolioValuationHandler(
    IBus bus,
    ILogger<ProcessCalculatePortfolioValuationHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculatePortfolioValuationCommand>
{
    public async Task HandleAsync(ProcessCalculatePortfolioValuationCommand command, CancellationToken ct = default)
    {
        var (valuationsRunId, pipelineMode, portfolioId, asOfDate) = command;
        
        var value = await CalculateValuation(portfolioId, asOfDate, ct);
        var changed = await UpsertValuation(portfolioId, asOfDate, value, ct);
        
        if (changed)
        {
            await context.SaveChangesAsync(ct);
            await bus.Publish(new PortfolioValuationChangedEvent(valuationsRunId, pipelineMode, portfolioId, asOfDate));
        }

        await bus.Publish(new PortfolioValuationCalculatedEvent(valuationsRunId, pipelineMode, portfolioId, asOfDate));
        
        logger.LogInformation("Portfolio calculated for Portfolio {PortfolioId}, AsOfDate {AsOfDate}", portfolioId, asOfDate.ToIsoDateString());
    }

    private async Task<bool> UpsertValuation(int portfolioId, DateOnly asOfDate, decimal value, CancellationToken ct)
    {
        var existing = await context.PortfolioValuations
            .FirstOrDefaultAsync(pv => pv.PortfolioId == portfolioId && pv.AsOfDate == asOfDate, cancellationToken: ct);

        if (existing == null)
        {
            var pv = new PortfolioValuation
                { AsOfDate = asOfDate, PortfolioId = portfolioId, UpdatedUtc = DateTime.UtcNow, Value = value };
            context.Add(pv);
            return true;
        }

        if (existing.Value != value)
        {
            existing.Value = value;
            existing.UpdatedUtc = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    private async Task<decimal> CalculateValuation(int portfolioId, DateOnly asOfDate, CancellationToken ct)
    {
        var portfolio = await context.Portfolios
            .AsNoTracking()
            .Include(x => x.Accounts)
            .FirstAsync(x => x.Id == portfolioId, ct);
        
        var accountIds = portfolio.Accounts.Select(a => a.Id).ToArray();

        return await context.HoldingValuations
            .AsNoTracking()
            .Where(x => accountIds.Contains(x.Holding.AccountId))
            .Where(x => x.Holding.AsOfDate == asOfDate)
            .Select(x => x.Value)
            .SumAsync(ct);
    }
}