using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record CloseOfDayAlertCommand(DateOnly AsOfDate) : ICommand;

public class CloseOfDayAlertHandler(
    HoardContext hoardContext,
    IEnumerable<IPortfolioAlertEvaluator> evaluators,
    IEnumerable<ICloseOfDayPortfolioAlertEvaluator> closeOfDayEvaluators,
    TimeProvider timeProvider)
    : ICommandHandler<CloseOfDayAlertCommand>
{
    public async Task HandleAsync(CloseOfDayAlertCommand command, CancellationToken ct = default)
    {
        if (command.AsOfDate != DateOnlyHelper.TodayLocal(timeProvider))
        {
            return;
        }

        if (!command.AsOfDate.IsWeekday())
        {
            return;
        }

        var portfolioPerformances = await hoardContext.PortfolioPerformances
            .Include(x => x.Portfolio)
            .ToListAsync(ct);

        foreach (var portfolioPerformance in portfolioPerformances)
        {
            foreach (var evaluator in evaluators)
            {
                await evaluator.EvaluateAsync(portfolioPerformance, command.AsOfDate, ct);
            }

            foreach (var evaluator in closeOfDayEvaluators)
            {
                await evaluator.EvaluateAsync(portfolioPerformance, command.AsOfDate, ct);
            }
        }
    }
}
