using Hoard.Core.Application.Portfolios;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class RebalanceServiceTests
{
    private readonly RebalanceService _service = new();

    [Fact]
    public void DeriveRebalanceActions_EmptyPortfolio_ReturnsEmptyList()
    {
        var result = _service.DeriveRebalanceActions(0m, []);

        Assert.Empty(result);
    }

    [Fact]
    public void DeriveRebalanceActions_NoDeviations_ReturnsEmptyList()
    {
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(1, "Stocks", 0m),
            CreateSubclass(2, "Bonds", 0m)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Empty(result);
    }

    [Fact]
    public void DeriveRebalanceActions_LargePositiveDeviation_AddsReduceAction()
    {
        // Portfolio value: 100,000
        // Reduce threshold: 1% = 1,000
        // Stocks deviation: +2,000 (over allocated)
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 5000m),
            CreateSubclass(1, "Stocks", 2000m)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        var reduceAction = Assert.Single(result);
        Assert.Equal(RebalanceActionType.Reduce, reduceAction.RebalanceAction);
        Assert.Equal(1, reduceAction.AssetSubclassId);
        Assert.Equal("Stocks", reduceAction.AssetSubclassName);
        Assert.Equal(2000m, reduceAction.Amount);
    }

    [Fact]
    public void DeriveRebalanceActions_DeviationBelowThreshold_NoReduceAction()
    {
        // Portfolio value: 100,000
        // Reduce threshold: 1% = 1,000
        // Stocks deviation: +500 (below threshold)
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 5000m),
            CreateSubclass(1, "Stocks", 500m)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Empty(result);
    }

    [Fact]
    public void DeriveRebalanceActions_LargeNegativeDeviation_AddsAddAction()
    {
        // Portfolio value: 100,000
        // Add threshold: -0.5% = -500
        // Bonds deviation: -2,000 (under allocated)
        // Cash: 5,000
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 5000m),
            CreateSubclass(2, "Bonds", -2000m, assetClassId: 20)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        var addAction = Assert.Single(result);
        Assert.Equal(RebalanceActionType.Add, addAction.RebalanceAction);
        Assert.Equal(20, addAction.AssetSubclassId);
        Assert.Equal("Bonds", addAction.AssetSubclassName);
        Assert.Equal(2000m, addAction.Amount);
    }

    [Fact]
    public void DeriveRebalanceActions_InsufficientCash_LimitsAddAmount()
    {
        // Portfolio value: 100,000
        // Bonds deviation: -5,000 (under allocated)
        // Cash: 2,000 (insufficient)
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 2000m),
            CreateSubclass(2, "Bonds", -5000m, assetClassId: 20)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        var addAction = Assert.Single(result);
        Assert.Equal(RebalanceActionType.Add, addAction.RebalanceAction);
        Assert.Equal(2000m, addAction.Amount); // Limited by available cash
    }

    [Fact]
    public void DeriveRebalanceActions_SmallPurchaseAmount_SkipsAction()
    {
        // Portfolio value: 100,000
        // Add threshold: -0.5% = -500
        // Bonds deviation: -2,000, but only 400 cash available
        // Amount (400) < threshold (500), should skip
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 400m),
            CreateSubclass(2, "Bonds", -2000m, assetClassId: 20)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Empty(result);
    }

    [Fact]
    public void DeriveRebalanceActions_MultipleSubclasses_ProcessesInOrder()
    {
        // Portfolio value: 100,000
        // Cash: 10,000
        // Multiple subclasses with negative deviations (ordered by deviation)
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 10000m),
            CreateSubclass(1, "Stocks", -3000m, assetClassId: 10),
            CreateSubclass(2, "Bonds", -1000m, assetClassId: 20),
            CreateSubclass(3, "Commodities", -500m, assetClassId: 30)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Equal(3, result.Count);

        // Should be ordered by deviation (most negative first)
        Assert.Equal("Stocks", result[0].AssetSubclassName);
        Assert.Equal(3000m, result[0].Amount);

        Assert.Equal("Bonds", result[1].AssetSubclassName);
        Assert.Equal(1000m, result[1].Amount);

        Assert.Equal("Commodities", result[2].AssetSubclassName);
        Assert.Equal(500m, result[2].Amount);
    }

    [Fact]
    public void DeriveRebalanceActions_ReduceIncreasesAvailableCash()
    {
        // Portfolio value: 100,000
        // Initial cash: 1,000
        // Stocks over-allocated by 3,000 -> sell, increases cash to 4,000
        // Bonds under-allocated by 2,000 -> can now buy with proceeds
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 1000m),
            CreateSubclass(1, "Stocks", 3000m),
            CreateSubclass(2, "Bonds", -2000m, assetClassId: 20)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Equal(2, result.Count);

        var reduceAction = result.First(x => x.RebalanceAction == RebalanceActionType.Reduce);
        Assert.Equal("Stocks", reduceAction.AssetSubclassName);
        Assert.Equal(3000m, reduceAction.Amount);

        var addAction = result.First(x => x.RebalanceAction == RebalanceActionType.Add);
        Assert.Equal("Bonds", addAction.AssetSubclassName);
        Assert.Equal(2000m, addAction.Amount); // Can buy full amount with proceeds
    }

    [Fact]
    public void DeriveRebalanceActions_StopsWhenCashRunsOut()
    {
        // Portfolio value: 100,000
        // Add threshold: -0.5% = -500
        // Cash: 1,500
        // Multiple under-allocated subclasses
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 1500m),
            CreateSubclass(1, "Stocks", -2000m, assetClassId: 10),
            CreateSubclass(2, "Bonds", -1000m, assetClassId: 20),
            CreateSubclass(3, "Commodities", -500m, assetClassId: 30)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        // Should buy Stocks (1500), then cash runs out and stops before small purchase
        Assert.Single(result);
        Assert.Equal("Stocks", result[0].AssetSubclassName);
        Assert.Equal(1500m, result[0].Amount);
    }

    [Fact]
    public void DeriveRebalanceActions_IgnoresCashInRebalancing()
    {
        // Cash itself should never appear in rebalance actions
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 2000m, value: 5000m), // Deviation shouldn't trigger action
            CreateSubclass(1, "Stocks", 1500m)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        var reduceAction = Assert.Single(result);
        Assert.NotEqual(AssetSubclass.Cash, reduceAction.AssetSubclassId);
        Assert.Equal("Stocks", reduceAction.AssetSubclassName);
    }

    [Fact]
    public void DeriveRebalanceActions_NoCashAvailable_NoAddActions()
    {
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(1, "Bonds", -2000m, assetClassId: 10)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        Assert.Empty(result);
    }

    [Fact]
    public void DeriveRebalanceActions_ExactThresholdDeviation_IncludesAction()
    {
        // Portfolio value: 100,000
        // Reduce threshold: 1% = 1,000
        // Stocks deviation: exactly 1,000
        var subclasses = new List<AssetSubclassExposureDto>
        {
            CreateSubclass(AssetSubclass.Cash, "Cash", 0m, value: 5000m),
            CreateSubclass(1, "Stocks", 1000m)
        };

        var result = _service.DeriveRebalanceActions(100000m, subclasses);

        var reduceAction = Assert.Single(result);
        Assert.Equal(RebalanceActionType.Reduce, reduceAction.RebalanceAction);
    }

    private static AssetSubclassExposureDto CreateSubclass(
        int id,
        string name,
        decimal deviationValue,
        int assetClassId = 1,
        decimal value = 0m)
    {
        return new AssetSubclassExposureDto
        {
            Id = id,
            Name = name,
            Colour = "#000000",
            AssetClassId = assetClassId,
            AssetClassName = "Test Class",
            AssetClassColour = "#111111",
            DeviationValue = deviationValue,
            Value = value
        };
    }
}
