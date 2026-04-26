using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Alerts;

public interface IPortfolioAlertEvaluator
{
    Task EvaluateAsync(PortfolioPerformance portfolioPerformance, DateOnly asOfDate, CancellationToken ct);
}
