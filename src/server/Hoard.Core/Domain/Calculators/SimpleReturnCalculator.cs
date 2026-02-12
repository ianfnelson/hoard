using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public class SimpleReturnCalculator : IReturnCalculator
{
    public decimal? Calculate(decimal startValue, decimal endValue, DateOnly startDate, DateOnly endDate, IList<Transaction> periodTransactions,
        PerformanceScope scope, bool annualised = false)
    {
        var periodWithdrawals = CalculateWithdrawals(periodTransactions, scope);
        var periodContributions = CalculateContributions(periodTransactions, scope);
        
        var numerator = endValue + periodWithdrawals - periodContributions - startValue;
        var denominator = startValue + periodContributions;

        var periodReturn = 100.0M * numerator / denominator;

        return !annualised ? periodReturn : AnnualisedReturnCalculator.Annualise(periodReturn, startDate, endDate);
    }

    private static decimal CalculateContributions(IList<Transaction> periodTransactions, PerformanceScope scope)
    {
        if (scope == PerformanceScope.Portfolio)
        {
            return periodTransactions
                .Where(t => TransactionTypeSets.Deposit.Contains(t.TransactionTypeId))
                .Sum(t => t.Value);
        }
        
        return periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Buy ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: < decimal.Zero })
            .Sum(t => -t.Value);
    }

    private static decimal CalculateWithdrawals(IList<Transaction> periodTransactions, PerformanceScope scope)
    {
        if (scope == PerformanceScope.Portfolio)
        {
            return periodTransactions
                .Where(t => t.TransactionTypeId == TransactionType.Withdrawal)
                .Sum(t => -t.Value);
        }
        
        return periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Sell || 
                        t.TransactionTypeId == TransactionType.IncomeDividend ||
                        t.TransactionTypeId == TransactionType.IncomeLoyaltyBonus ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: > decimal.Zero })
            .Sum(t => t.Value);
    }
}