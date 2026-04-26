using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record PortfolioPerformanceAlertCommand(int PortfolioId, DateOnly AsOfDate) : ICommand;

public class PortfolioPerformanceAlertHandler(
    HoardContext hoardContext,
    IEnumerable<IPortfolioAlertEvaluator> evaluators)
    : ICommandHandler<PortfolioPerformanceAlertCommand>
{
    public async Task HandleAsync(PortfolioPerformanceAlertCommand command, CancellationToken ct = default)
    {
        var portfolioPerformance = await hoardContext.PortfolioPerformances
            .Include(x => x.Portfolio)
            .FirstOrDefaultAsync(x => x.PortfolioId == command.PortfolioId, ct);

        if (portfolioPerformance is null)
        {
            return;
        }

        foreach (var evaluator in evaluators)
        {
            await evaluator.EvaluateAsync(portfolioPerformance, command.AsOfDate, ct);
        }
    }
}
