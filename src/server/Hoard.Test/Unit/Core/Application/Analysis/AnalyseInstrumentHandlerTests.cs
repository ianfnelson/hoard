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
    public void CalculateVolatility_FewerThan250Prices_ReturnsNull()
    {
        var prices = CreatePricesWithCloses(Enumerable.Repeat(100m, 249).ToArray());

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.Null(result);
    }

    [Fact]
    public void CalculateVolatility_ConstantPrices_ReturnsZero()
    {
        var prices = CreatePricesWithCloses(Enumerable.Repeat(100m, 250).ToArray());

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateVolatility_KnownValues_ReturnsCorrectStdDev()
    {
        var closes = Enumerable.Repeat(100m, 248).Concat([102, 101, 103, 100]).ToArray();
        var prices = CreatePricesWithCloses(closes);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.NotNull(result);
        Assert.True(result > 0m);
        Assert.True(result < 10m);
    }

    [Fact]
    public void CalculateVolatility_SkipsZeroCloses()
    {
        var closes = new[] { 0m }.Concat(Enumerable.Repeat(100m, 250)).Concat([102]).ToArray();
        var prices = CreatePricesWithCloses(closes);

        var result = ProcessAnalyseInstrumentHandler.CalculateVolatility(prices);

        Assert.NotNull(result);
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
