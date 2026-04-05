using Hoard.Core.Application.Analysis;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Analysis;

public class GetInstrumentsForAnalysisHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsHeldInstrumentsWithPriceDataFeed()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Accounts.Add(TestData.Account());

        var instrument1 = new Instrument { Id = 1, Name = "Stock A", Ticker = "SA", CurrencyId = "GBP" };
        instrument1.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Prices,
            Configuration = new DataFeedConfiguration { Ticker = "SA" }
        });
        var instrument2 = new Instrument { Id = 2, Name = "Stock B", Ticker = "SB", CurrencyId = "GBP" };
        // No price data feed

        context.Instruments.AddRange(instrument1, instrument2);

        var today = new DateOnly(2024, 6, 15);
        context.Holdings.AddRange(
            new Holding { Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = today, Units = 100, UpdatedUtc = DateTime.UtcNow },
            new Holding { Id = 2, AccountId = 1, InstrumentId = 2, AsOfDate = today, Units = 50, UpdatedUtc = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForAnalysisHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForAnalysisQuery(today));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_NoHoldings_ReturnsEmpty()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsForAnalysisHandler(context);

        var result = await handler.HandleAsync(new GetInstrumentsForAnalysisQuery(new DateOnly(2024, 1, 1)));

        Assert.Empty(result);
    }
}
