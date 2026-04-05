using Hoard.Core.Application.Quotes;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Quotes;

public class GetInstrumentsForQuoteRefreshHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsInstrumentsWithQuoteDataFeed()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var instrument1 = new Instrument { Id = 1, Name = "Stock A", Ticker = "SA", CurrencyId = "GBP" };
        instrument1.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "SA" }
        });
        var instrument2 = new Instrument { Id = 2, Name = "Stock B", Ticker = "SB", CurrencyId = "GBP" };
        // No quote data feed

        context.Instruments.AddRange(instrument1, instrument2);
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForQuoteRefreshHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForQuoteRefreshQuery());

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsForQuoteRefreshHandler(context);

        var result = await handler.HandleAsync(new GetInstrumentsForQuoteRefreshQuery());

        Assert.Empty(result);
    }
}
