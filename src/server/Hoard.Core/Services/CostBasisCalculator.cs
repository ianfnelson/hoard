using Hoard.Core.Domain;

namespace Hoard.Core.Services;

public static class CostBasisCalculator
{
    public static (decimal CostBasis, decimal RealisedGain) Calculate(
        IEnumerable<CashflowRecord> flows)
    {
        var totalUnits = decimal.Zero;
        var totalCost = decimal.Zero;
        var realisedGain = decimal.Zero;

        foreach (var f in flows.OrderBy(x => x.Date))
        {
            switch (f.CategoryId)
            {
                case TransactionCategory.Buy:
                {
                    totalUnits += f.Units!.Value;
                    totalCost  += f.Amount;
                    break;
                }

                case TransactionCategory.Sell:
                {
                    var unitsSold = f.Units!.Value;
                    var saleProceeds = -f.Amount;

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
                    totalCost  += f.Amount;
                    break;
                }
            }
        }

        return (totalCost, realisedGain);
    }
}