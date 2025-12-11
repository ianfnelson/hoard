using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Positions;

public record ProcessCalculatePositionsCommand(Guid CorrelationId, PipelineMode PipelineMode) : ICommand;

public class ProcessCalculatePositionsHandler(
    ILogger<ProcessCalculatePositionsHandler> logger, HoardContext context, IBus bus)
    : ICommandHandler<ProcessCalculatePositionsCommand>
{
    public async Task HandleAsync(ProcessCalculatePositionsCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Calculating positions");
        
        var portfolios = await context.Portfolios
            .Include(p => p.Accounts)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var portfolio in portfolios)
        {
            await CalculatePositionsForPortfolioAsync(portfolio, ct);
        }
        
        await bus.Publish(new PositionsCalculatedEvent(command.CorrelationId, command.PipelineMode));
    }
    
    private async Task CalculatePositionsForPortfolioAsync(Portfolio portfolio, CancellationToken ct = default)
    {
        logger.LogInformation("Calculating positions for portfolio {PortfolioId}", portfolio.Id);

        // 1. Figure out which accounts are in this portfolio
        var accountIds = portfolio.Accounts.Select(a => a.Id).ToList();

        // 2. Load and aggregate holdings (read-only, can be big, done OUTSIDE tx)
        var holdingsList = await context.Holdings
            .AsNoTracking()
            .Where(h => accountIds.Contains(h.AccountId))
            .Where(h => h.InstrumentId != Instrument.Cash)
            .ToListAsync(ct);

        var aggregatedHoldings = holdingsList
            .GroupBy(h => new { h.InstrumentId, h.AsOfDate })
            .Select(g => new AggregatedHolding(
                g.Key.InstrumentId,
                g.Key.AsOfDate,
                g.Sum(h => h.Units)
            ))
            .OrderBy(x => x.InstrumentId)
            .ThenBy(x => x.AsOfDate)
            .ToList();

        var positionsToInsert = BuildPositions(portfolio.Id, aggregatedHoldings);

        // 3. Atomic swap: delete old positions, insert new, inside ONE transaction
        await using var tx = await context.Database.BeginTransactionAsync(ct);

        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM Position WHERE PortfolioId = {portfolio.Id}", ct);

            context.Positions.AddRange(positionsToInsert);
            await context.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            logger.LogInformation(
                "Calculated {Count} positions for portfolio {PortfolioId}",
                positionsToInsert.Count, portfolio.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating positions for portfolio {PortfolioId}", portfolio.Id);
            await tx.RollbackAsync(ct);
            throw;
        }
    }
    
    private static List<Position> BuildPositions(int portfolioId, List<AggregatedHolding> holdings)
    {
        var result = new List<Position>();

        var maxDate = holdings.Max(h => h.AsOfDate);
        
        var byInstrument = holdings
            .GroupBy(h => h.InstrumentId)
            .ToList();

        foreach (var g in byInstrument)
        {
            var instrumentId = g.Key;
            bool open = false;
            DateOnly? openDate = null;

            var rows = g.OrderBy(x => x.AsOfDate).ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                var current = rows[i];
                var currentUnits = current.Units;

                var isLast = i == rows.Count - 1;
                var nextUnits = isLast ? 0 : rows[i + 1].Units;
                DateOnly? nextDate = isLast ? null : rows[i + 1].AsOfDate;
                var hasGap = nextDate != null && nextDate.Value > current.AsOfDate.AddDays(1);

                bool isToday = current.AsOfDate == maxDate;

                // Open stint
                if (!open && currentUnits > 0)
                {
                    open = true;
                    openDate = current.AsOfDate;
                }

                // Close stint only if (not today) AND (units zero OR a gap)
                if (open && !isToday && (nextUnits == 0 || hasGap))
                {
                    result.Add(new Position
                    {
                        PortfolioId = portfolioId,
                        InstrumentId = instrumentId,
                        OpenDate = openDate!.Value,
                        CloseDate = current.AsOfDate.AddDays(1)
                    });

                    open = false;
                    openDate = null;
                }
            }
            
            var last = rows[^1];

            if (open && openDate is not null && last.AsOfDate == maxDate)
            {
                result.Add(new Position
                {
                    PortfolioId = portfolioId,
                    InstrumentId = instrumentId,
                    OpenDate = openDate.Value,
                    CloseDate = null
                });
            }
        }

        return result;
    }

    private record AggregatedHolding(int InstrumentId, DateOnly AsOfDate, decimal Units);
}