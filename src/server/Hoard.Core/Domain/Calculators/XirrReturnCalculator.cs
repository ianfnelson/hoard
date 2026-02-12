using Excel.FinancialFunctions;
using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public class XirrReturnCalculator : IReturnCalculator
{
    public decimal? Calculate(
        decimal startValue,
        decimal endValue,
        DateOnly startDate,
        DateOnly endDate,
        IList<Transaction> periodTransactions,
        PerformanceScope scope,
        bool annualised
    )
    {
        var transactionTypes = scope == PerformanceScope.Portfolio ? 
            TransactionTypeSets.PortfolioCashflows : TransactionTypeSets.PositionCashflows;
        
        var cashflows = new List<CashFlow>();

        if (startValue != 0)
        {
            cashflows.Add(new CashFlow(startDate.ToDateTime(TimeOnly.MinValue), -(double)startValue));
        }

        cashflows.AddRange(
            periodTransactions
                .Where(t => transactionTypes.Contains(t.TransactionTypeId))
                .Select(t => new CashFlow(t.Date.ToDateTime(TimeOnly.MinValue),
                    scope == PerformanceScope.Portfolio ? -(double)t.Value : (double)t.Value))
                .OrderBy(x => x.Date)
        );

        if (endValue != 0)
        {
            cashflows.Add(new CashFlow(endDate.ToDateTime(TimeOnly.MinValue), (double)endValue));
        }

        var values = cashflows.Select(x => x.Value);
        var dates = cashflows.Select(x => x.Date);

        var xirr = Financial.XIrr(values, dates);
        var annualisedReturn = (decimal)(xirr * 100);
        
        return annualised ? annualisedReturn : AnnualisedReturnCalculator.Deannualise(annualisedReturn, startDate, endDate);
    }

    private record CashFlow(DateTime Date, double Value);
}