using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public static class MoneyWeightedReturnCalculator
{
    public static decimal Calculate(decimal currentValue, IList<Transaction> transactions)
    {
        var contributions = GetContributions(transactions);
        var gain = GetGain(transactions);

        var numerator = currentValue + gain;
        var denominator = contributions;
        
        return denominator == decimal.Zero ? decimal.Zero : 100.0M * numerator / denominator;
    }

    private static decimal GetGain(IList<Transaction> transactions)
    {
        var gain = transactions
            .Select(t => t.Value)
            .Sum();
        return gain;
    }

    private static decimal GetContributions(IList<Transaction> transactions)
    {
        var contributions = transactions
            .Where(t => t.CategoryId == TransactionCategory.Buy ||
                        t is { CategoryId: TransactionCategory.CorporateAction, Value: < 0 })
            .Select(t => -t.Value)
            .Sum();
        return contributions;
    }
}