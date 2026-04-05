using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hoard.Test.Unit.Core.Domain.Calculators;

public class HybridReturnCalculatorTests
{
    private readonly Mock<ILogger<HybridReturnCalculator>> _mockLogger;
    private readonly SimpleReturnCalculator _simpleCalculator;
    private readonly XirrReturnCalculator _xirrCalculator;
    private readonly HybridReturnCalculator _calculator;

    public HybridReturnCalculatorTests()
    {
        _mockLogger = new Mock<ILogger<HybridReturnCalculator>>();
        _simpleCalculator = new SimpleReturnCalculator();
        _xirrCalculator = new XirrReturnCalculator();
        _calculator = new HybridReturnCalculator(_mockLogger.Object, _simpleCalculator, _xirrCalculator);
    }

    [Theory]
    [InlineData(2023, 1, 1, 2023, 1, 1)]     // zero days
    [InlineData(2023, 12, 31, 2023, 1, 1)]   // negative days
    public void Calculate_ZeroOrNegativeDays_ReturnsNull(
        int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay)
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(startYear, startMonth, startDay),
            endDate: new DateOnly(endYear, endMonth, endDay),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.Null(result);
    }

    [Fact]
    public void Calculate_LessThan30DaysAnnualised_ReturnsNull()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 1, 29),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.Null(result);
    }

    [Theory]
    [InlineData(2023, 1, 29)]  // 28 days
    [InlineData(2023, 1, 30)]  // 29 days
    public void Calculate_LessThan30DaysNotAnnualised_UsesSimpleCalculator(
        int endYear, int endMonth, int endDay)
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(endYear, endMonth, endDay),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        Assert.Equal(10m, result.Value, 2);
    }

    [Fact]
    public void Calculate_Exactly30Days_UsesXirrCalculator()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 1, 31),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // 10% over 30 days annualised via XIRR — very high annualised rate
        Assert.True(result.Value > 100m, $"Expected large annualised return for 30-day 10% gain, got {result.Value}");
    }

    [Fact]
    public void Calculate_MoreThan30Days_UsesXirrCalculator()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // 10% over ~1 year via XIRR = ~10% annualised
        Assert.Equal(10.0m, result.Value, 0);
    }

    [Fact]
    public void Calculate_XirrThrowsException_FallsBackToSimpleCalculator()
    {
        var mockXirr = new Mock<XirrReturnCalculator>();
        mockXirr
            .Setup(x => x.Calculate(
                It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), It.IsAny<PerformanceScope>(),
                It.IsAny<bool>()))
            .Throws(new InvalidOperationException("XIRR convergence failed"));

        var calculator = new HybridReturnCalculator(_mockLogger.Object, _simpleCalculator, mockXirr.Object);

        var result = calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // Falls back to simple calculator: (1100 - 1000) / 1000 * 100 = 10%, annualised over ~1 year ≈ 10%
        Assert.Equal(10.0m, result.Value, 0);
    }

    [Fact]
    public void Calculate_XirrFails_LogsWarning()
    {
        var mockXirr = new Mock<XirrReturnCalculator>();
        mockXirr
            .Setup(x => x.Calculate(
                It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IList<Transaction>>(), It.IsAny<PerformanceScope>(),
                It.IsAny<bool>()))
            .Throws(new InvalidOperationException("XIRR convergence failed"));

        var calculator = new HybridReturnCalculator(_mockLogger.Object, _simpleCalculator, mockXirr.Object);

        calculator.Calculate(
            startValue: 1000m,
            endValue: 1100m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 12, 31),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    [Fact]
    public void Calculate_PortfolioScope_UsesXirrWithPortfolioCashflows()
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
            annualised: true
        );

        Assert.NotNull(result);
        // 1000 start + 500 mid-year deposit -> 1600 end = ~7.8% annualised via XIRR
        Assert.Equal(7.8m, result.Value, 1);
    }

    [Fact]
    public void Calculate_PositionScope_UsesXirrWithPositionCashflows()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Value = -500m,
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
            annualised: true
        );

        Assert.NotNull(result);
        // 1000 start + 500 mid-year buy -> 1600 end = ~7.8% annualised via XIRR
        Assert.Equal(7.8m, result.Value, 1);
    }

    [Fact]
    public void Calculate_ShortPeriodWithTransactions_UsesSimpleCalculator()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.DepositPersonal,
                Value = 100m,
                Date = new DateOnly(2023, 1, 15)
            }
        };

        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1110m,
            startDate: new DateOnly(2023, 1, 1),
            endDate: new DateOnly(2023, 1, 20),
            periodTransactions: transactions,
            scope: PerformanceScope.Portfolio,
            annualised: false
        );

        Assert.NotNull(result);
        // Simple: (1110 + 0 - 100 - 1000) / (1000 + 100) * 100 = 10/1100*100 ≈ 0.91%
        Assert.Equal(0.9m, result.Value, 1);
    }


    [Fact]
    public void Calculate_LongPeriodWithNoTransactions_UsesXirr()
    {
        var result = _calculator.Calculate(
            startValue: 1000m,
            endValue: 1200m,
            startDate: new DateOnly(2020, 1, 1),
            endDate: new DateOnly(2023, 1, 1),
            periodTransactions: new List<Transaction>(),
            scope: PerformanceScope.Portfolio,
            annualised: true
        );

        Assert.NotNull(result);
        // 20% over 3 years = ~6.3% annualised
        Assert.Equal(6.3m, result.Value, 1);
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
