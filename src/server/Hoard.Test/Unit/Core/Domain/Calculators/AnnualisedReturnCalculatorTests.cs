using Hoard.Core.Domain.Calculators;

namespace Hoard.Test.Unit.Core.Domain.Calculators;

public class AnnualisedReturnCalculatorTests
{
    [Fact]
    public void Annualise_NullReturn_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Annualise(null, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Annualise_ZeroDays_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Annualise(10m, new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Annualise_NegativeDays_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Annualise(10m, new DateOnly(2024, 1, 1), new DateOnly(2023, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Annualise_TotalWipeout_ReturnsNegative100()
    {
        var result = AnnualisedReturnCalculator.Annualise(-100m, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.Equal(-100m, result);
    }

    [Fact]
    public void Annualise_PathologicalNegativeReturn_ReturnsNegative100()
    {
        var result = AnnualisedReturnCalculator.Annualise(-150m, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.Equal(-100m, result);
    }

    [Fact]
    public void Annualise_OneYearPeriod_ReturnsSameValue()
    {
        var result = AnnualisedReturnCalculator.Annualise(50m, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.NotNull(result);
        Assert.Equal(50m, result.Value, 1); // Allow 1 decimal place tolerance
    }

    [Fact]
    public void Annualise_TwoYearPeriod_ReturnsAnnualisedValue()
    {
        var result = AnnualisedReturnCalculator.Annualise(100m, new DateOnly(2023, 1, 1), new DateOnly(2025, 1, 1));

        Assert.NotNull(result);
        // 100% over 2 years ≈ 41.4% annualised (compound)
        Assert.Equal(41.4m, result.Value, 1);
    }

    [Fact]
    public void Annualise_SixMonthPeriod_ReturnsHigherAnnualisedValue()
    {
        var result = AnnualisedReturnCalculator.Annualise(10m, new DateOnly(2023, 1, 1), new DateOnly(2023, 7, 1));

        Assert.NotNull(result);
        // 10% over ~6 months (181 days) should annualize to ~21.2%
        Assert.True(result.Value > 20m && result.Value < 22m);
    }

    [Fact]
    public void Annualise_VeryLargeReturn_CapTo10000()
    {
        var result = AnnualisedReturnCalculator.Annualise(10000m, new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 10));

        Assert.NotNull(result);
        Assert.Equal(10000m, result.Value);
    }

    [Fact]
    public void Annualise_VeryLargeNegativeReturn_CapToNegative100()
    {
        // -99.9% over 6 months annualises to effectively -100% (growth near zero raised to power > 1)
        var result = AnnualisedReturnCalculator.Annualise(-99.9m, new DateOnly(2023, 1, 1), new DateOnly(2023, 7, 1));

        Assert.NotNull(result);
        Assert.Equal(-100m, result.Value, 0);
    }

    [Fact]
    public void Deannualise_NullReturn_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Deannualise(null, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Deannualise_ZeroDays_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Deannualise(10m, new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Deannualise_NegativeDays_ReturnsNull()
    {
        var result = AnnualisedReturnCalculator.Deannualise(10m, new DateOnly(2024, 1, 1), new DateOnly(2023, 1, 1));

        Assert.Null(result);
    }

    [Fact]
    public void Deannualise_OneYearPeriod_ReturnsSameValue()
    {
        var result = AnnualisedReturnCalculator.Deannualise(50m, new DateOnly(2023, 1, 1), new DateOnly(2024, 1, 1));

        Assert.NotNull(result);
        Assert.Equal(50m, result.Value, 1);
    }

    [Fact]
    public void Deannualise_TwoYearPeriod_ReturnsCompoundedValue()
    {
        var result = AnnualisedReturnCalculator.Deannualise(20m, new DateOnly(2023, 1, 1), new DateOnly(2025, 1, 1));

        Assert.NotNull(result);
        // 20% annualized over 2 years = 44% total (1.2^2 - 1)
        Assert.Equal(44m, result.Value, 1);
    }

    [Fact]
    public void Deannualise_SixMonthPeriod_ReturnsLowerPeriodValue()
    {
        var result = AnnualisedReturnCalculator.Deannualise(20m, new DateOnly(2023, 1, 1), new DateOnly(2023, 7, 1));

        Assert.NotNull(result);
        // 20% annualized over ~6 months should be ~9.5%
        Assert.True(result.Value > 9m && result.Value < 10.5m);
    }

    [Fact]
    public void AnnualiseAndDeannualise_RoundTrip_ReturnsOriginalValue()
    {
        var periodReturn = 50m;
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2025, 1, 1);

        var annualised = AnnualisedReturnCalculator.Annualise(periodReturn, startDate, endDate);
        var deannualised = AnnualisedReturnCalculator.Deannualise(annualised, startDate, endDate);

        Assert.NotNull(deannualised);
        Assert.Equal(periodReturn, deannualised.Value, 1);
    }
}
