using Hoard.Core.Domain.Calculators;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Domain.Calculators;

public class CostBasisCalculatorTests
{
    [Fact]
    public void Calculate_EmptyTransactions_ReturnsZero()
    {
        var transactions = new List<Transaction>();

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(0m, costBasis);
        Assert.Equal(0m, realisedGain);
    }

    [Fact]
    public void Calculate_SingleBuy_ReturnsCostBasis()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(1000m, costBasis);
        Assert.Equal(0m, realisedGain);
    }

    [Fact]
    public void Calculate_MultipleBuys_AccumulatesCost()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 50m,
                Value = -600m,
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(1600m, costBasis);
        Assert.Equal(0m, realisedGain);
    }

    [Theory]
    [InlineData(600, 500, 100)]    // sell at profit (£12/unit)
    [InlineData(400, 500, -100)]   // sell at loss (£8/unit)
    public void Calculate_BuyAndSell_CalculatesRealisedGain(
        decimal sellValue, decimal expectedCostBasis, decimal expectedGain)
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m, // £10 per unit
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -50m,
                Value = sellValue,
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(expectedCostBasis, costBasis);
        Assert.Equal(expectedGain, realisedGain);
    }

    [Fact]
    public void Calculate_SellWithoutBuy_ThrowsException()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -50m,
                Value = 600m,
                Date = new DateOnly(2023, 1, 1)
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => 
            CostBasisCalculator.Calculate(transactions));
        
        Assert.Equal("Sell flow with no units.", exception.Message);
    }

    [Fact]
    public void Calculate_CorporateAction_AddsToCostBasis()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Units = 50m, // e.g., stock split or bonus shares
                Value = 0m,
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        // Cost basis stays at £1000, now spread over 150 units
        Assert.Equal(1000m, costBasis);
        Assert.Equal(0m, realisedGain);
    }

    [Fact]
    public void Calculate_CorporateActionWithCost_AddsToCostBasis()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Units = 50m,
                Value = -100m, // e.g., rights issue
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(1100m, costBasis);
        Assert.Equal(0m, realisedGain);
    }

    [Fact]
    public void Calculate_AveragesCostBasisAcrossPurchases()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m, // £10 per unit
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1500m, // £15 per unit
                Date = new DateOnly(2023, 2, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -100m,
                Value = 1400m, // £14 per unit
                Date = new DateOnly(2023, 3, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        // Average cost: £2500 / 200 = £12.50 per unit
        // Sold 100 units at £12.50 = £1250 cost
        Assert.Equal(1250m, costBasis);
        // £1400 proceeds - £1250 cost = £150 gain
        Assert.Equal(150m, realisedGain);
    }

    [Fact]
    public void Calculate_OrdersByDate_ProducesCorrectResult()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -50m,
                Value = 600m,
                Date = new DateOnly(2023, 3, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        // Should process in date order: buy first, then sell
        Assert.Equal(500m, costBasis);
        Assert.Equal(100m, realisedGain);
    }

    [Fact]
    public void Calculate_ComplexScenario_HandlesMultipleTransactions()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 100m,
                Value = -1000m,
                Date = new DateOnly(2023, 1, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -50m,
                Value = 550m,
                Date = new DateOnly(2023, 2, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.CorporateAction,
                Units = 25m,
                Value = 0m,
                Date = new DateOnly(2023, 3, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Buy,
                Units = 50m,
                Value = -600m,
                Date = new DateOnly(2023, 4, 1)
            },
            new()
            {
                TransactionTypeId = TransactionType.Sell,
                Units = -75m,
                Value = 900m,
                Date = new DateOnly(2023, 5, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        // Buy 100 @ £10: 100 units, £1000 cost
        // Sell 50 @ £11: gain = 550 - 500 = £50. 50 units, £500 cost
        // Corporate action +25 units @ £0: 75 units, £500 cost
        // Buy 50 @ £12: 125 units, £1100 cost (average £8.80/unit)
        // Sell 75 @ £12: costRemoved = 8.80 * 75 = £660, gain = 900 - 660 = £240. 50 units, £440 cost
        Assert.Equal(440m, costBasis);
        Assert.Equal(290m, realisedGain);
    }
}
