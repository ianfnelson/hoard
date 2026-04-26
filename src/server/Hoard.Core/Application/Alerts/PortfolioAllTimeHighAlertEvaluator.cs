using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;

namespace Hoard.Core.Application.Alerts;

public sealed class PortfolioAllTimeHighAlertEvaluator(
    HoardContext hoardContext,
    IAlertService alertService) : ICloseOfDayPortfolioAlertEvaluator
{
    public async Task EvaluateAsync(
        PortfolioPerformance portfolioPerformance, DateOnly asOfDate, CancellationToken ct)
    {
        var portfolio = portfolioPerformance.Portfolio;
        var currentValue = portfolioPerformance.Value;

        if (portfolio.AllTimeHighValue.HasValue && currentValue <= portfolio.AllTimeHighValue.Value)
        {
            return;
        }

        var raised = await alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.PortfolioAllTimeHigh,
            TriggeredDate = asOfDate,
            PortfolioId = portfolio.Id,
            SuppressionKey = $"{AlertType.PortfolioAllTimeHigh}:{portfolio.Id}:{asOfDate.ToIsoDateString()}",
            Title = $"{portfolio.Name} — New All-Time High",
            Message = $"{portfolio.Name} closed at a new all-time high of £{currentValue:F2}.",
            Url = HoardUrls.PortfolioOverview(portfolio.Id),
            UrlTitle = portfolio.Name
        }, ct);

        if (!raised)
        {
            return;
        }

        portfolio.AllTimeHighValue = currentValue;
        portfolio.AllTimeHighDate = asOfDate;
        await hoardContext.SaveChangesAsync(ct);
    }
}
