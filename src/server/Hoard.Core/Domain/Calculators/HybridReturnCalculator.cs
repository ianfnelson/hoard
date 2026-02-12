using Hoard.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Domain.Calculators;

public interface IReturnCalculator
{
    decimal? Calculate(
        decimal startValue,
        decimal endValue,
        DateOnly startDate,
        DateOnly endDate,
        IList<Transaction> periodTransactions,
        PerformanceScope scope,
        bool annualised = false);
}

public class HybridReturnCalculator(
    ILogger<HybridReturnCalculator> logger,
    SimpleReturnCalculator simpleReturnCalculator,
    XirrReturnCalculator xirrReturnCalculator)
    : IReturnCalculator
{
    public decimal? Calculate(decimal startValue, decimal endValue, DateOnly startDate, DateOnly endDate,
        IList<Transaction> periodTransactions, PerformanceScope scope, bool annualised = false)
    {
        var days = endDate.DayNumber - startDate.DayNumber;

        if (days <= 0)
        {
            return null;
        }

        if (annualised && days < 30)
        {
            return null;
        }

        if (days < 30)
        {
            return simpleReturnCalculator.Calculate(startValue, endValue, startDate, endDate, periodTransactions, scope, annualised);
        }

        try
        {
            return xirrReturnCalculator.Calculate(startValue, endValue, startDate, endDate, periodTransactions, scope,
                annualised);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "XIRR calculation failed for {Scope} from {StartDate} to {EndDate} ({Days} days, {TransactionCount} transactions). Falling back to simple return calculator",
                scope, startDate, endDate, days, periodTransactions.Count);

            return simpleReturnCalculator.Calculate(startValue, endValue, startDate, endDate, periodTransactions, scope, annualised);
        }
    }
}

public enum PerformanceScope
{
    Position, 
    Portfolio
}