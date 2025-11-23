namespace Hoard.Core.Services;

public sealed class ModifiedDietzCalculator
{
    /// <summary>
    /// Compute Modified Dietz return for a given period.
    /// </summary>
    /// <param name="startValue">Value at period start.</param>
    /// <param name="endValue">Value at period end.</param>
    /// <param name="incomeValue">Income paid out during the period.</param>
    /// <param name="cashflows">
    /// External cashflows only. Positive = contribution INTO portfolio/position; negative = withdrawal.
    /// Each entry must have (Date, Amount).
    /// </param>
    /// <param name="startDate">Period start date (inclusive).</param>
    /// <param name="endDate">Period end date (exclusive or inclusive; consistent use is fine).</param>
    /// <returns>Modified Dietz return for the period.</returns>
    public static decimal Calculate(
        decimal startValue,
        decimal endValue,
        decimal incomeValue,
        DateOnly startDate,
        DateOnly endDate,
        IEnumerable<CashflowRecord> cashflows)
    {
        if (startDate == endDate)
        {
            return decimal.Zero;
        }

        var days = endDate.DayNumber - startDate.DayNumber;
        if (days <= 0)
        {
            return decimal.Zero;
        }

        var weightedFlowSum = 0m;
        var flowSum = 0m;

        foreach (var cf in cashflows)
        {
            if (cf.Date <= startDate || cf.Date > endDate)
            {
                continue;
            }

            var daysAfterStart = (decimal)(cf.Date.DayNumber - startDate.DayNumber); 
            var totalDays = (decimal)days;
            
            var weight = (totalDays - daysAfterStart) / totalDays;

            weightedFlowSum += cf.Amount * weight;
            flowSum += cf.Amount;
        }

        var numerator = endValue + incomeValue - startValue - flowSum;
        var denominator = startValue + weightedFlowSum;

        return denominator == decimal.Zero ? decimal.Zero : 100.0M * numerator / denominator;
    }
}