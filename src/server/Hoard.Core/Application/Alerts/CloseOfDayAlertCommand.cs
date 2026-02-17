using System.Text;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record CloseOfDayAlertCommand(DateOnly AsOfDate) : ICommand;

public class CloseOfDayAlertHandler(HoardContext hoardContext, IAlertService alertService) 
    : ICommandHandler<CloseOfDayAlertCommand>
{
    public async Task HandleAsync(CloseOfDayAlertCommand command, CancellationToken ct = default)
    {
        if (command.AsOfDate != DateOnlyHelper.TodayLocal())
        {
            return;
        }

        if (!command.AsOfDate.IsWeekday())
        {
            return;
        }

        var alertRequests = await BuildAlertRequests(command.AsOfDate, ct);
        foreach (var alertRequest in alertRequests)
        {
            await alertService.TryRaiseAsync(alertRequest, ct);
        }
    }

    private async Task<IEnumerable<AlertRequest>> BuildAlertRequests(DateOnly asOfDate, CancellationToken ct)
    {
        var portfolioPerformances = await hoardContext.PortfolioPerformances
            .Include(x => x.Portfolio)
            .AsNoTracking()
            .ToListAsync(ct);

        return portfolioPerformances.Select(p => BuildAlertRequest(p, asOfDate));
    }

    private static AlertRequest BuildAlertRequest( PortfolioPerformance portfolioPerformance, DateOnly asOfDate)
    {
        return new AlertRequest
        {
            AlertTypeId = AlertType.CloseOfDayPortfolioValuation,
            TriggeredDate = asOfDate,
            PortfolioId = portfolioPerformance.PortfolioId,
            SuppressionKey = BuildSuppressionKey(portfolioPerformance, asOfDate),
            Title = BuildTitle(portfolioPerformance, asOfDate),
            Message = BuildMessage(portfolioPerformance),
            Url = BuildUrl(portfolioPerformance),
            UrlTitle = BuildUrlTitle(portfolioPerformance)
        };
    }

    private static string BuildUrlTitle(PortfolioPerformance portfolioPerformance)
    {
        return portfolioPerformance.Portfolio.Name;
    }

    private static string BuildUrl(PortfolioPerformance portfolioPerformance)
    {
        return $"http://hoard.iannelson.uk/portfolios/{portfolioPerformance.PortfolioId}/overview";
    }

    private static string BuildSuppressionKey(PortfolioPerformance portfolioPerformance, DateOnly asOfDate)
    {
        return $"{AlertType.CloseOfDayPortfolioValuation}:{portfolioPerformance.PortfolioId}:{asOfDate}";
    }
    
    private static string BuildTitle(PortfolioPerformance portfolioPerformance, DateOnly asOfDate)
    {
        return $"{portfolioPerformance.Portfolio.Name} close of day {asOfDate.ToIsoDateString()}";
    }

    private static string BuildMessage(PortfolioPerformance portfolioPerformance)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Valuation: £{portfolioPerformance.Value:F2}");
        sb.AppendLine($"Change: £{portfolioPerformance.ValueChange:F2}");
        sb.AppendLine($"Day: {portfolioPerformance.Return1D:F2}%");
        sb.AppendLine($"Year: {portfolioPerformance.Return1Y:F2}%");
        
        return sb.ToString();
    }
}