namespace Hoard.Core.Domain.Calculators;

public static class AnnualisedReturnCalculator
{
    public static decimal? Annualise(decimal? returnPercentage, DateOnly startDate, DateOnly endDate)
    {
        if (!returnPercentage.HasValue)
        {
            return null;
        }
        
        var growth = 1.0 + (double)returnPercentage.Value / 100.0;
        
        // total wipeout or pathological negative return?
        if (growth <= 0.0)
        {
            return -100m;
        }

        var days = endDate.DayNumber - startDate.DayNumber;
        var years = days / 365.25;

        if (years <= 0.0)
        {
            return null;
        }

        var annualisedGrowth = Math.Pow(growth, 1.0 / years);
        var annualisedReturn = (annualisedGrowth - 1.0) * 100.0;

        const double upperCap = 10000.0;
        const double lowerCap = -100.0;

        if (double.IsNaN(annualisedReturn) || double.IsInfinity(annualisedReturn) || annualisedReturn > upperCap)
        {
            annualisedReturn = upperCap;
        }
        else if (annualisedReturn < lowerCap)
        {
            annualisedReturn = lowerCap;
        }

        return (decimal)annualisedReturn;
    }

    public static decimal? Deannualise(decimal? returnPercentage, DateOnly startDate, DateOnly endDate)
    {
        if (!returnPercentage.HasValue)
        {
            return null;
        }
        
        var days = endDate.DayNumber - startDate.DayNumber;
        var years = days / 365.25;

        if (years <= 0.0)
        {
            return null;
        }
        
        var annualisedGrowth = 1 + (double)returnPercentage.Value / 100.0;
        var periodGrowth = Math.Pow(annualisedGrowth, years);
        
        var periodReturn = (periodGrowth - 1.0) * 100.0;
        
        return (decimal)periodReturn;
    }
    
}