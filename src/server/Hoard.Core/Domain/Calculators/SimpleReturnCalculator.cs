using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public static class SimpleReturnCalculator
{
    public static decimal CalculateForPosition(decimal valueStart, decimal valueEnd, IList<Transaction> periodTransactions)
    {
        var periodWithdrawals = periodTransactions
            .Where(t => t.CategoryId == TransactionCategory.Sell || 
                        t.CategoryId == TransactionCategory.Income ||
                        t is { CategoryId: TransactionCategory.CorporateAction, Value: > decimal.Zero })
            .Sum(t => t.Value);
        
        var periodContributions = periodTransactions
            .Where(t => t.CategoryId == TransactionCategory.Buy ||
                        t is { CategoryId: TransactionCategory.CorporateAction, Value: < decimal.Zero })
            .Sum(t => -t.Value);
        
        return Calculate(valueStart, valueEnd, periodWithdrawals, periodContributions);
    }

    public static decimal CalculateForPortfolio(decimal valueStart, decimal valueEnd, IList<Transaction> periodTransactions)
    {
        var periodWithdrawals = periodTransactions
            .Where(t => t.CategoryId == TransactionCategory.Withdrawal)
            .Sum(t => -t.Value);
        
        var periodContributions = periodTransactions
            .Where(t => t.CategoryId == TransactionCategory.Deposit)
            .Sum(t => t.Value);
        
        return Calculate(valueStart, valueEnd, periodWithdrawals, periodContributions);
    }

    public static decimal Calculate(decimal valueStart, decimal valueEnd, decimal periodWithdrawals, decimal periodContributions)
    {   
        var numerator = valueEnd + periodWithdrawals - periodContributions - valueStart;
        var denominator = valueStart + periodContributions;
        
        return denominator == decimal.Zero ? decimal.Zero : 100.0M * numerator / denominator;
    }
}