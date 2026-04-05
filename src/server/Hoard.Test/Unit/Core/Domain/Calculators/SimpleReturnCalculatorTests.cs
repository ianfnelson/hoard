using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Domain.Calculators;

public class SimpleReturnCalculatorTests
{
    private readonly SimpleReturnCalculator _calculator = new();

    [Theory]
    [InlineData(1000, 1100, 10)]    // positive return
    [InlineData(1000, 800, -20)]    // negative return
    public void Calculate_NoTransactions_ReturnsSimpleReturn(
        decimal startValue, decimal endValue, decimal expectedReturn)
    {
        var result = _calculator.Calculate(
            startValue: startValue,
            endValue: endValue,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        Assert.Equal(expectedReturn, result.Value, 2);
    }

    [Fact]
    public void Calculate_PortfolioScopeWithDeposit_IncludesContribution()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 500m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1600m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // (1600 + 0 - 500 - 1000) / (1000 + 500) * 100 = 100 / 1500 * 100 = 6.67%
        Assert.Equal(6.67m, result.Value, 1);
    }

    [Fact]
    public void Calculate_PortfolioScopeWithWithdrawal_IncludesWithdrawal()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Withdrawal,
                Value = -300m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 800m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // (800 + 300 - 0 - 1000) / (1000 + 0) * 100 = 100 / 1000 * 100 = 10%
        Assert.Equal(10m, result.Value, 2);
    }

    [Fact]
    public void Calculate_PositionScopeWithBuy_IncludesContribution()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Value = -500m, // Buy has negative value
                Units = 50m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1600m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: false
        );

        Assert.NotNull(result);
        // (1600 + 0 - 500 - 1000) / (1000 + 500) * 100 = 100 / 1500 * 100 = 6.67%
        Assert.Equal(6.67m, result.Value, 1);
    }

    [Fact]
    public void Calculate_PositionScopeWithSell_IncludesWithdrawal()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Value = 300m,
                Units = -30m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 800m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: false
        );

        Assert.NotNull(result);
        // (800 + 300 - 0 - 1000) / (1000 + 0) * 100 = 100 / 1000 * 100 = 10%
        Assert.Equal(10m, result.Value, 2);
    }

    [Theory]
    [InlineData(TransactionType.IncomeDividend, 50, 5)]
    [InlineData(TransactionType.IncomeLoyaltyBonus, 25, 2.5)]
    public void Calculate_PositionScopeWithIncome_IncludesWithdrawal(
        int transactionTypeId, decimal incomeValue, decimal expectedReturn)
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = transactionTypeId,
                Value = incomeValue,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1000m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: false
        );

        Assert.NotNull(result);
        Assert.Equal(expectedReturn, result.Value, 2);
    }

    [Fact]
    public void Calculate_PositionScopeWithCorporateActionPositiveValue_IncludesWithdrawal()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Value = 100m,
                Units = 10m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1000m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: false
        );

        Assert.NotNull(result);
        // (1000 + 100 - 0 - 1000) / (1000 + 0) * 100 = 100 / 1000 * 100 = 10%
        Assert.Equal(10m, result.Value, 2);
    }

    [Fact]
    public void Calculate_PositionScopeWithCorporateActionNegativeValue_IncludesContribution()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Value = -100m,
                Units = 10m,
                Date = new DateOnly(2023, 6, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Position,
            annualised: false
        );

        Assert.NotNull(result);
        // (1100 + 0 - 100 - 1000) / (1000 + 100) * 100 = 0 / 1100 * 100 = 0%
        Assert.Equal(0m, result.Value, 2);
    }

    [Fact]
    public void Calculate_WithAnnualised_CallsAnnualisedCalculator()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1210m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2024, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Simple return is 21%, over 1 year should stay ~21%
        Assert.Equal(21m, result.Value, 1);
    }

    [Fact]
    public void Calculate_MultipleDepositTypes_AllCountedAsContributions()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 100m,
                Date = new DateOnly(2023, 2, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.DepositEmployer,
                Value = 50m,
                Date = new DateOnly(2023, 3, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.DepositTransfer,
                Value = 75m,
                Date = new DateOnly(2023, 4, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.DepositIncomeTaxReclaim,
                Value = 25m,
                Date = new DateOnly(2023, 5, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1300m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // Total contributions: 250
        // (1300 + 0 - 250 - 1000) / (1000 + 250) * 100 = 50 / 1250 * 100 = 4%
        Assert.Equal(4m, result.Value, 1);
    }


    [Fact]
    public void Calculate_ComplexScenario_ReturnsCorrectValue()
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
                TransactionTypeId = TransactionType.Withdrawal,
                Value = -200m,
                Date = new DateOnly(2023, 9, 1)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1400m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // (1400 + 200 - 500 - 1000) / (1000 + 500) * 100 = 100 / 1500 * 100 = 6.67%
        Assert.Equal(6.67m, result.Value, 1);
    }
}
