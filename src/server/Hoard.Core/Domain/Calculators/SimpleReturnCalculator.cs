using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public static class SimpleReturnCalculator
{
    public static decimal CalculateForPosition(decimal valueStart, decimal valueEnd, IList<Transaction> periodTransactions)
    {
        var periodWithdrawals = periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Sell || 
                        t.TransactionTypeId == TransactionType.IncomeDividend ||
                        t.TransactionTypeId == TransactionType.IncomeLoyaltyBonus ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: > decimal.Zero })
            .Sum(t => t.Value);
        
        var periodContributions = periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Buy ||
                        t is { TransactionTypeId: TransactionType.CorporateAction, Value: < decimal.Zero })
            .Sum(t => -t.Value);
        
        return Calculate(valueStart, valueEnd, periodWithdrawals, periodContributions);
    }

    public static decimal CalculateForPortfolio(decimal valueStart, decimal valueEnd, IList<Transaction> periodTransactions)
    {
        var periodWithdrawals = periodTransactions
            .Where(t => t.TransactionTypeId == TransactionType.Withdrawal)
            .Sum(t => -t.Value);
        
        var periodContributions = periodTransactions
            .Where(t => TransactionTypeSets.Deposit.Contains(t.TransactionTypeId))
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