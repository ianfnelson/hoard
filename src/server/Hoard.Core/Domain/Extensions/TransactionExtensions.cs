using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Extensions;

internal static class TransactionExtensions
{
    internal static List<CashflowRecord> ToPortfolioCashflows(this IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => 
                t.CategoryId is TransactionCategory.Deposit or TransactionCategory.Withdrawal)
            .Select(t => new CashflowRecord(t.Date, t.Value, null, t.CategoryId))
            .ToList();
    }

    internal static List<CashflowRecord> ToPositionCashflows(this IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => 
                t.CategoryId is TransactionCategory.Buy or TransactionCategory.Sell or TransactionCategory.CorporateAction)
            .Select(t => new CashflowRecord(t.Date, -t.Value, t.Units, t.CategoryId))
            .ToList();
    }
}