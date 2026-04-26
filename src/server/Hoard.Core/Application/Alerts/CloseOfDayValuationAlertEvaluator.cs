using System.Text;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;

namespace Hoard.Core.Application.Alerts;

public sealed class CloseOfDayValuationAlertEvaluator(IAlertService alertService) : IPortfolioAlertEvaluator
{
    public Task EvaluateAsync(
        PortfolioPerformance portfolioPerformance, DateOnly asOfDate, CancellationToken ct)
    {
        var portfolio = portfolioPerformance.Portfolio;

        var message = new StringBuilder()
            .AppendLine($"Valuation: £{portfolioPerformance.Value:F2}")
            .AppendLine($"Change: £{portfolioPerformance.ValueChange1D:F2}")
            .AppendLine($"Day: {portfolioPerformance.Return1D:F2}%")
            .AppendLine($"Year: {portfolioPerformance.Return1Y:F2}%")
            .ToString();

        return alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.CloseOfDayPortfolioValuation,
            TriggeredDate = asOfDate,
            PortfolioId = portfolio.Id,
            SuppressionKey = $"{AlertType.CloseOfDayPortfolioValuation}:{portfolio.Id}:{asOfDate.ToIsoDateString()}",
            Title = $"{portfolio.Name} close of day {asOfDate.ToIsoDateString()}",
            Message = message,
            Url = HoardUrls.PortfolioOverview(portfolio.Id),
            UrlTitle = portfolio.Name
        }, ct);
    }
}
