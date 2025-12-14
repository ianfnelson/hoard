using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Domain.Calculators;

public static class CostBasisCalculator
{
    public static (decimal CostBasis, decimal RealisedGain) Calculate(
        IEnumerable<Transaction> transactions)
    {
        var totalUnits = decimal.Zero;
        var totalCost = decimal.Zero;
        var realisedGain = decimal.Zero;

        foreach (var f in transactions.OrderBy(x => x.Date))
        {
            switch (f.CategoryId)
            {
                case TransactionCategory.Buy:
                {
                    totalUnits += f.Units!.Value;
                    totalCost  -= f.Value;
                    break;
                }

                case TransactionCategory.Sell:
                {
                    var unitsSold = f.Units!.Value;
                    var saleProceeds = f.Value;

                    if (totalUnits <= 0)
                    {
                        throw new InvalidOperationException("Sell flow with no units.");
                    }

                    var averageCost = totalCost / totalUnits;

                    var costRemoved = averageCost * unitsSold;
                    realisedGain += saleProceeds - costRemoved;

                    totalUnits -= unitsSold;
                    totalCost  -= costRemoved;

                    if (totalUnits == decimal.Zero)
                    {
                        totalUnits = decimal.Zero;
                    }

                    break;
                }

                case TransactionCategory.CorporateAction:
                {
                    totalUnits += f.Units!.Value;
                    totalCost  -= f.Value;
                    break;
                }
            }
        }

        return (totalCost, realisedGain);
    }
}