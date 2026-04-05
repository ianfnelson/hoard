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
        Assert.Equal(10.0m, result.Value, 1);
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
        // 1000 start + 1000 mid-year deposit -> 2200 end = ~13.4% annualised
        Assert.Equal(13.4m, result.Value, 1);
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
        // 1000 start - 500 mid-year withdrawal -> 600 end = ~13.2% annualised
        Assert.Equal(13.2m, result.Value, 1);
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
        // 1000 start + 1000 mid-year buy -> 2200 end = ~13.4% annualised
        Assert.Equal(13.4m, result.Value, 1);
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
        // 1000 start - 500 mid-year sell -> 600 end = ~13.2% annualised
        Assert.Equal(13.2m, result.Value, 1);
    }

    [Theory]
    [InlineData(TransactionType.IncomeDividend, 50, 1050, 10.3)]
    [InlineData(TransactionType.IncomeLoyaltyBonus, 25, 1025, 5.1)]
    public void Calculate_PositionScopeWithIncome_CalculatesXirr(
        int transactionTypeId, decimal incomeValue, decimal endValue, decimal expectedReturn)
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = transactionTypeId,
                Value = incomeValue,
                Date = new DateOnly(2023, 7, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: endValue,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: true
        );

        Assert.NotNull(result);
        Assert.Equal(expectedReturn, result.Value, 1);
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
        // 1000 start + 100 mid-year corporate action cost -> 1150 end = ~4.8% annualised
        Assert.Equal(4.8m, result.Value, 1);
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
        // 21% over 2 years = 10% annualised
        Assert.Equal(10.0m, result.Value, 1);
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
        // 10% over 2 years, de-annualised back to period return = ~10.0%
        Assert.Equal(10.0m, result.Value, 1);
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
        // Deposited 1000 on Jan 15, ended at 1100 = ~10.4% annualised
        Assert.Equal(10.4m, result.Value, 1);
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
        // Withdrew full amount at same value = 0% return
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
        // Net invested 1400, ended at 1500 with time-weighted cashflows = ~7.0% annualised
        Assert.Equal(7.0m, result.Value, 1);
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
        // 1000 -> 800 over 1 year = -20%
        Assert.Equal(-20.0m, result.Value, 1);
    }
}
