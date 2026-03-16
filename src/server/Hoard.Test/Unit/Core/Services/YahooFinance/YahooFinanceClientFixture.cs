using Hoard.Core.Infrastructure.Integrations;

namespace Hoard.Test.Unit.Core.Services.YahooFinance;

public class YahooFinanceClientFixture
{
    [Fact]
    public async Task GetQuotesAsync_CanGetMultipleQuotes()
    {
        // Arrange
        var sut = new Hoard.Core.Infrastructure.Integrations.YahooFinance();

        // Act
        var quotes = await sut.GetQuotesAsync(["AAPL", "MSFT"]);

        // Assert
        Assert.Equal(2, quotes.Count);

        var apple = quotes["AAPL"];
        Assert.True(apple.Bid > 0);
        Assert.True(apple.Ask > 0);

        var microsoft = quotes["MSFT"];
        Assert.True(microsoft.Bid > 0);
        Assert.True(microsoft.Ask > 0);
    }
    
    [Fact]
    public async Task GetQuotesAsync_CanGetLondonQuotes()
    {
        // Arrange
        var sut = new Hoard.Core.Infrastructure.Integrations.YahooFinance();

        // Act
        var quotes = await sut.GetQuotesAsync(["FWRG.L"]);

        // Assert
        var fwrg = quotes["FWRG.L"];
        Assert.Contains("Invesco FTSE All-World", fwrg.Name);
        Assert.Equal("Yahoo! Finance", fwrg.Source);
    }

    [Fact]
    public async Task GetPricesAsync_CanGetPrices()
    {
        // Arrange
        var sut = new Hoard.Core.Infrastructure.Integrations.YahooFinance();

        // Act
        var prices = await sut.GetPricesAsync("FWRG.L", new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 8));

        // Assert
        Assert.Equal(6, prices.Count);
        Assert.Equal("Yahoo! Finance", prices[0].Source);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 1) && x.Open == 598.0M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 2) && x.High == 607.526M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 3) && x.Low == 600.5M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 6) && x.Close == 608.0M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 7) && x.AdjustedClose == 607.4M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025, 10, 8) && x.Volume == 1111832L);
    }

    [Fact]
    public async Task GetPricesAsync_DeduplicatesByDate()
    {
        // Arrange
        var sut = new Hoard.Core.Infrastructure.Integrations.YahooFinance();

        // Act
        var prices = await sut.GetPricesAsync("AAPL", new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 8));

        // Assert
        // Verify no duplicate dates exist (Yahoo Finance API has been observed to return duplicates)
        var uniqueDates = prices.Select(p => p.Date).Distinct().Count();
        Assert.Equal(prices.Count, uniqueDates);
    }

    [Fact]
    public void DeduplicatePrices_PrefersNonZeroClose_WhenDuplicateDatesExist()
    {
        // Arrange - first entry has zero Close, second has valid Close
        var prices = new List<PriceDto>
        {
            new() { Date = new DateOnly(2025, 10, 1), Open = 100, High = 110, Low = 90, Close = 0, Volume = 1000, AdjustedClose = 0, Source = "Yahoo! Finance" },
            new() { Date = new DateOnly(2025, 10, 1), Open = 100, High = 110, Low = 90, Close = 105, Volume = 2000, AdjustedClose = 105, Source = "Yahoo! Finance" },
            new() { Date = new DateOnly(2025, 10, 2), Open = 200, High = 210, Low = 190, Close = 205, Volume = 3000, AdjustedClose = 205, Source = "Yahoo! Finance" },
        };

        // Act
        var result = Hoard.Core.Infrastructure.Integrations.YahooFinance.DeduplicatePrices(prices);

        // Assert
        Assert.Equal(2, result.Count);
        var oct1 = result.Single(p => p.Date == new DateOnly(2025, 10, 1));
        Assert.Equal(105, oct1.Close);
        Assert.Equal(2000, oct1.Volume);
    }

    [Fact]
    public void DeduplicatePrices_ReturnsFirst_WhenAllDuplicatesHaveZeroClose()
    {
        // Arrange - both entries have zero Close, should return first
        var prices = new List<PriceDto>
        {
            new() { Date = new DateOnly(2025, 10, 1), Open = 100, High = 110, Low = 90, Close = 0, Volume = 1000, AdjustedClose = 0, Source = "Yahoo! Finance" },
            new() { Date = new DateOnly(2025, 10, 1), Open = 101, High = 111, Low = 91, Close = 0, Volume = 2000, AdjustedClose = 0, Source = "Yahoo! Finance" },
        };

        // Act
        var result = Hoard.Core.Infrastructure.Integrations.YahooFinance.DeduplicatePrices(prices);

        // Assert
        Assert.Single(result);
        Assert.Equal(1000, result[0].Volume);
    }

    [Fact]
    public void DeduplicatePrices_ReturnsAllUnique_WhenNoDuplicatesExist()
    {
        // Arrange
        var prices = new List<PriceDto>
        {
            new() { Date = new DateOnly(2025, 10, 1), Open = 100, High = 110, Low = 90, Close = 105, Volume = 1000, AdjustedClose = 105, Source = "Yahoo! Finance" },
            new() { Date = new DateOnly(2025, 10, 2), Open = 200, High = 210, Low = 190, Close = 205, Volume = 2000, AdjustedClose = 205, Source = "Yahoo! Finance" },
        };

        // Act
        var result = Hoard.Core.Infrastructure.Integrations.YahooFinance.DeduplicatePrices(prices);

        // Assert
        Assert.Equal(2, result.Count);
    }
}