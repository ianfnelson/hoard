using Hoard.Core.Domain;
using Hoard.Core.Services;

namespace Hoard.Core.Extensions;

internal static class TransactionExtensions
{
    internal static List<CashflowRecord> ToPortfolioCashflows(this IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => 
                t.CategoryId == TransactionCategory.Deposit || 
                t.CategoryId == TransactionCategory.Withdrawal)
            .Select(t => new CashflowRecord(t.Date, t.Value, null, t.CategoryId))
            .ToList();
    }

    internal static List<CashflowRecord> ToPositionCashflows(this IEnumerable<Transaction> transactions)
    {
        return transactions
            .Where(t => 
                t.CategoryId == TransactionCategory.Buy || 
                t.CategoryId == TransactionCategory.Sell ||
                (t.CategoryId == TransactionCategory.CorporateAction && t.Value!=decimal.Zero))
            .Select(t => new CashflowRecord(t.Date, -t.Value, t.Units, t.CategoryId))
            .ToList();
    }
}