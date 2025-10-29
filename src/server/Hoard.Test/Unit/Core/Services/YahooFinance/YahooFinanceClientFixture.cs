using Hoard.Core.Services.YahooFinance;

namespace Hoard.Test.Unit.Core.Services.YahooFinance;

public class YahooFinanceClientFixture
{
    [Fact]
    public async Task GetQuotesAsync_CanGetMultipleQuotes()
    {
        // Arrange
        var sut = new YahooFinanceClient();

        // Act
        var quotes = await sut.GetQuotesAsync(["AAPL", "MSFT"]);

        // Assert
        Assert.Equal(2, quotes.Count);
        
        var apple = quotes.Single(x => x.Symbol == "AAPL");
        Assert.True(apple.Bid > 0);
        Assert.True(apple.Ask > 0);
        
        var microsoft = quotes.Single(x => x.Symbol == "MSFT");
        Assert.True(microsoft.Bid > 0);
        Assert.True(microsoft.Ask > 0);
    }
    
    [Fact]
    public async Task GetQuotesAsync_CanGetLondonQuotes()
    {
        // Arrange
        var sut = new YahooFinanceClient();

        // Act
        var quotes = await sut.GetQuotesAsync(["FWRG.L"]);

        // Assert
        var fwrg = quotes.Single();
        Assert.Contains("Invesco FTSE All-World", fwrg.Name);
        Assert.Equal("Yahoo! Finance", fwrg.Source);
    }

    [Fact]
    public async Task GetHistoricalAsync_CanGetHistoricalPrices()
    {
        // Arrange
        var sut = new YahooFinanceClient();
        
        // Act
        var prices = await sut.GetHistoricalAsync("FWRG.L", new DateOnly(2025,10,1), new DateOnly(2025,10,8));
        
        // Assert
        Assert.Equal(6, prices.Count);
        Assert.Equal("Yahoo! Finance", prices[0].Source);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,1) && x.Open == 598.0M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,2) && x.High == 607.53M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,3) && x.Low == 600.5M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,6) && x.Close == 608.0M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,7) && x.AdjustedClose == 607.4M);
        Assert.Contains(prices, x => x.Date == new DateOnly(2025,10,8) && x.Volume == 1111832L);
    }
}