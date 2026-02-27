using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Domain.Calculators;

public class XirrReturnCalculatorTests
{
    private readonly XirrReturnCalculator _calculator = new();

    [Fact]
    public void Calculate_NoTransactions_ReturnsSimpleReturn()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Should return approximately 10% annualized
        Assert.True(result.Value > 9m && result.Value < 11m);
    }

    [Fact]
    public void Calculate_PortfolioScopeWithDeposits_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 1000m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 2200m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // With mid-year deposit, return should be positive
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PortfolioScopeWithWithdrawal_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Withdrawal,
                Value = -500m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 600m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // With mid-year withdrawal, return should be positive (100 gain)
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PositionScopeWithBuy_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Value = -1000m,
                Units = 100m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 2200m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        // With mid-year purchase and good growth, return should be positive
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PositionScopeWithSell_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Value = 500m,
                Units = -50m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 600m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        // With mid-year sale and remaining value, return should be positive
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PositionScopeWithDividend_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.IncomeDividend,
                Value = 50m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1050m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        // Dividend income should contribute to positive return
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PositionScopeWithLoyaltyBonus_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.IncomeLoyaltyBonus,
                Value = 25m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1025m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        // Loyalty bonus should contribute to positive return
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_PositionScopeWithCorporateAction_CalculatesXirr()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Value = -100m,
                Units = 10m,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1150m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        // Corporate action with cost should be handled correctly
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_Annualised_ReturnsAnnualisedRate()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1210m,
            startDate: new DateOnly(2021, 1, 1),
            endDate: new DateOnly(2023, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // 21% over 2 years = ~10% annualized
        Assert.True(result.Value > 9m && result.Value < 11m);
    }

    [Fact]
    public void Calculate_NotAnnualised_ReturnsPeriodRate()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2021, 1, 1),
            endDate: new DateOnly(2023, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // Should return period return, not annualized
        Assert.True(result.Value > 9m && result.Value < 11m);
    }

    [Fact]
    public void Calculate_ZeroStartValue_HandlesCorrectly()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 1000m,
                Date = new DateOnly(2023, 1, 15)
            }
        };

        var result = _calculator.Calculate(
            startValue: 0m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Started with nothing, deposited 1000, ended with 1100 = 10% gain
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_ZeroEndValue_HandlesCorrectly()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Withdrawal,
                Value = -1000m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 0m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Withdrew everything at same value = 0% return
        Assert.Equal(0m, result.Value, 1);
    }

    [Fact]
    public void Calculate_MultipleTransactions_HandlesCorrectly()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 500m,
                Date = new DateOnly(2023, 3, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.DepositEmployer,
                Value = 200m,
                Date = new DateOnly(2023, 6, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Withdrawal,
                Value = -300m,
                Date = new DateOnly(2023, 9, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1500m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Multiple cashflows should be handled correctly
        Assert.True(result.Value > 0m);
    }

    [Fact]
    public void Calculate_NegativeReturn_HandlesCorrectly()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 800m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Should return negative value for loss
        Assert.True(result.Value < 0m);
        Assert.True(result.Value > -25m && result.Value < -15m);
    }
}
