using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.News;

public class GetInstrumentsForNewsRefreshHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithSpecificInstrumentId_ReturnsOnlyThatInstrument()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        SeedInstrumentWithNewsFeed(context, 1, "Stock A");
        SeedInstrumentWithNewsFeed(context, 2, "Stock B");
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForNewsRefreshHandler(context);

        // Act
        var result = await handler.HandleAsync(
            new GetInstrumentsForNewsRefreshQuery(InstrumentId: 1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_OnlyReturnsInstrumentsWithNewsDataFeed()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        SeedInstrumentWithNewsFeed(context, 1, "Stock A");
        context.Instruments.Add(new Instrument { Id = 2, Name = "Stock B", Ticker = "SB", CurrencyId = "GBP" });
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForNewsRefreshHandler(context);

        // Act
        var result = await handler.HandleAsync(
            new GetInstrumentsForNewsRefreshQuery(null, Messages.PipelineMode.Backfill));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    private static void SeedInstrumentWithNewsFeed(HoardContext context, int id, string name)
    {
        var instrument = new Instrument { Id = id, Name = name, Ticker = $"T{id}", CurrencyId = "GBP" };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = id,
            DataFeedId = DataFeed.News,
            Configuration = new DataFeedConfiguration { Ticker = $"T{id}" }
        });
        context.Instruments.Add(instrument);
    }
}
