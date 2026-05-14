using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;

namespace Hoard.Core.Application.Alerts;

public sealed class PortfolioDrawdownAlertEvaluator(IAlertService alertService) : IPortfolioAlertEvaluator
{
    private static readonly (int AlertTypeId, decimal Threshold)[] Thresholds =
    [
        (AlertType.PortfolioDrawdown10, 0.10m),
        (AlertType.PortfolioDrawdown20, 0.20m),
        (AlertType.PortfolioDrawdown30, 0.30m),
        (AlertType.PortfolioDrawdown50, 0.50m),
    ];

    public async Task EvaluateAsync(
        PortfolioPerformance portfolioPerformance, DateOnly asOfDate, CancellationToken ct)
    {
        var portfolio = portfolioPerformance.Portfolio;

        if (!portfolio.AllTimeHighValue.HasValue || !portfolio.AllTimeHighDate.HasValue)
        {
            return;
        }

        var allTimeHigh = portfolio.AllTimeHighValue.Value;
        if (allTimeHigh <= 0)
        {
            return;
        }

        var drawdown = (allTimeHigh - portfolioPerformance.Value) / allTimeHigh;
        var allTimeHighDate = portfolio.AllTimeHighDate.Value;

        foreach (var (alertTypeId, threshold) in Thresholds)
        {
            if (drawdown < threshold)
            {
                continue;
            }

            var thresholdPercent = (int)(threshold * 100);

            await alertService.TryRaiseAsync(new AlertRequest
            {
                AlertTypeId = alertTypeId,
                TriggeredDate = asOfDate,
                PortfolioId = portfolio.Id,
                SuppressionKey = $"{alertTypeId}:{portfolio.Id}:{allTimeHighDate.ToIsoDateString()}",
                Title = $"{portfolio.Name} — Drawdown Exceeds {thresholdPercent}%",
                Message = $"{portfolio.Name} has fallen {drawdown:P1} from its all-time high of £{allTimeHigh:N2}.",
                Url = HoardUrls.PortfolioOverview(portfolio.Id),
                UrlTitle = portfolio.Name
            }, ct);
        }
    }
}
