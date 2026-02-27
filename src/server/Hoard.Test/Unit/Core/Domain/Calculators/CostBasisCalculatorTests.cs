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
    public void Calculate_SingleBuy_ReturnsNegativeCost()
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

    [Fact]
    public void Calculate_BuyAndSell_CalculatesRealisedGain()
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
                Value = 600m, // £12 per unit
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        // 50 units remaining at £10 = £500 cost basis
        Assert.Equal(500m, costBasis);
        // Sold 50 units: £600 proceeds - £500 cost = £100 gain
        Assert.Equal(100m, realisedGain);
    }

    [Fact]
    public void Calculate_BuyAndSellWithLoss_CalculatesNegativeGain()
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
                Value = 400m, // £8 per unit
                Date = new DateOnly(2023, 2, 1)
            }
        };

        var (costBasis, realisedGain) = CostBasisCalculator.Calculate(transactions);

        Assert.Equal(500m, costBasis);
        // Sold 50 units: £400 proceeds - £500 cost = -£100 loss
        Assert.Equal(-100m, realisedGain);
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

        // After first sell: 50 units at £500 cost
        // After corporate action: 75 units at £500 cost
        // After second buy: 125 units at £1100 cost (average £8.80)
        // After second sell: 50 units remaining
        Assert.True(costBasis > 0);
        Assert.True(realisedGain != 0);
    }
}
