using Hoard.Core.Data;
using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Positions;

public record ProcessRebuildPositionsCommand(Guid CorrelationId) : ICommand;

public class ProcessRebuildPositionsHandler(ILogger<ProcessRebuildPositionsHandler> logger, HoardContext context)
    : ICommandHandler<ProcessRebuildPositionsCommand>
{
    public async Task HandleAsync(ProcessRebuildPositionsCommand command, CancellationToken ct = default)
    {
        logger.LogInformation("Rebuilding positions");
        
        var portfolios = await context.Portfolios
            .Include(p => p.Accounts)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var portfolio in portfolios)
        {
            await RebuildPositionsForPortfolioAsync(portfolio, ct);
        }
    }
    
    private async Task RebuildPositionsForPortfolioAsync( Portfolio portfolio, CancellationToken ct = default)
    {
        logger.LogInformation("Rebuilding positions for portfolio {PortfolioId}", portfolio.Id);

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
                "Rebuilt {Count} positions for portfolio {PortfolioId}",
                positionsToInsert.Count, portfolio.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rebuilding positions for portfolio {PortfolioId}", portfolio.Id);
            await tx.RollbackAsync(ct);
            throw;
        }
    }
    
    private static List<Position> BuildPositions(int portfolioId, List<AggregatedHolding> holdings)
    {
        var result = new List<Position>();

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
                var nextUnits = i == rows.Count - 1 ? 0 : rows[i + 1].Units;

                if (!open && currentUnits > 0)
                {
                    open = true;
                    openDate = current.AsOfDate;
                }

                if (open && nextUnits == 0)
                {
                    result.Add(new Position
                    {
                        PortfolioId = portfolioId,
                        InstrumentId = instrumentId,
                        OpenDate = openDate!.Value,
                        CloseDate = current.AsOfDate
                    });

                    open = false;
                    openDate = null;
                }
            }
        }

        return result;
    }

    private record AggregatedHolding(int InstrumentId, DateOnly AsOfDate, decimal Units);
}