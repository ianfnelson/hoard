using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Alerts;

public interface ICloseOfDayPortfolioAlertEvaluator
{
    Task EvaluateAsync(PortfolioPerformance portfolioPerformance, DateOnly asOfDate, CancellationToken ct);
}
