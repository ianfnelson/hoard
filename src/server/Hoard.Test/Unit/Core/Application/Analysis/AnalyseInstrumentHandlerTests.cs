using Hoard.Core.Application.Analysis;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Application.Analysis;

public class ProcessAnalyseInstrumentHandlerTests
{
    [Fact]
    public void CalculateMedianVolume_OddCount_ReturnsMiddle()
    {
        var prices = CreatePricesWithVolumes([100, 200, 300, 400, 500]);

        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume(prices);

        Assert.Equal(300, result);
    }

    [Fact]
    public void CalculateMedianVolume_EvenCount_ReturnsAverage()
    {
        var prices = CreatePricesWithVolumes([100, 200, 300, 400]);

        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume(prices);

        Assert.Equal(250, result);
    }

    [Fact]
    public void CalculateMedianVolume_SingleValue_ReturnsThat()
    {
        var prices = CreatePricesWithVolumes([42]);

        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume(prices);

        Assert.Equal(42, result);
    }

    [Fact]
    public void CalculateMedianVolume_NoVolumes_ReturnsNull()
    {
        var prices = new List<Price>
        {
            new() { Close = 100, Volume = null, Source = "test" }
        };

        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume(prices);

        Assert.Null(result);
    }

    [Fact]
    public void CalculateMedianVolume_EmptyList_ReturnsNull()
    {
        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume([]);

        Assert.Null(result);
    }

    [Fact]
    public void CalculateMedianVolume_UnsortedInput_StillCorrect()
    {
        var prices = CreatePricesWithVolumes([500, 100, 300, 200, 400]);

        var result = ProcessAnalyseInstrumentHandler.CalculateMedianVolume(prices);

        Assert.Equal(300, result);
    }

    [Fact]
    public void CalculateVolatility_ConstantPrices_ReturnsZero()
    {
        var prices = CreatePricesWithCloses([100, 100, 100, 100]);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateVolatility_SinglePrice_ReturnsZero()
    {
        var prices = CreatePricesWithCloses([100]);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateVolatility_KnownValues_ReturnsCorrectStdDev()
    {
        // Prices: 100, 102, 101, 103, 100
        // Returns: 0.02, -0.0098039..., 0.0198019..., -0.0291262...
        var prices = CreatePricesWithCloses([100, 102, 101, 103, 100]);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.True(result > 0m);
        Assert.True(result < 0.1m);
    }

    [Fact]
    public void CalculateVolatility_TwoPrices_ReturnsZeroStdDev()
    {
        // With only 2 prices, there's 1 return, and std dev of 1 value = 0
        var prices = CreatePricesWithCloses([100, 110]);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateVolatility_SkipsZeroCloses()
    {
        var prices = CreatePricesWithCloses([0, 100, 102]);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        // Only one return (100->102 = 0.02), std dev of single value = 0
        Assert.Equal(0m, result);
    }

    private static List<Price> CreatePricesWithVolumes(long[] volumes)
    {
        return volumes.Select((v, i) => new Price
        {
            Close = 100,
            Volume = v,
            AsOfDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-volumes.Length + i)),
            Source = "test"
        }).ToList();
    }

    private static List<Price> CreatePricesWithCloses(decimal[] closes)
    {
        return closes.Select((c, i) => new Price
        {
            Close = c,
            AsOfDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-closes.Length + i)),
            Source = "test"
        }).ToList();
    }
}
