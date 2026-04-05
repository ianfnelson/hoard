using Hoard.Core.Application.News;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.News;

public class GetNewsArticlesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ToDateIncludesArticlesPublishedOnThatDay()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var instrument = new Instrument { Id = 1, Name = "Test Co", Ticker = "TST", CurrencyId = "GBP" };
        context.Instruments.Add(instrument);
        context.NewsArticles.AddRange(
            CreateArticle(1, instrument, new DateTime(2026, 4, 2, 9, 30, 0, DateTimeKind.Utc)),
            CreateArticle(2, instrument, new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc)),
            CreateArticle(3, instrument, new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc))
        );
        await context.SaveChangesAsync();

        var handler = new GetNewsArticlesHandler(context);
        var query = new GetNewsArticlesQuery
        {
            FromDate = new DateOnly(2026, 4, 2),
            ToDate = new DateOnly(2026, 4, 2)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(2, item.PublishedUtc.Day));
    }

    [Fact]
    public async Task HandleAsync_FromDateIncludesArticlesPublishedOnThatDay()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var instrument = new Instrument { Id = 1, Name = "Test Co", Ticker = "TST", CurrencyId = "GBP" };
        context.Instruments.Add(instrument);
        context.NewsArticles.AddRange(
            CreateArticle(1, instrument, new DateTime(2026, 4, 1, 23, 59, 0, DateTimeKind.Utc)),
            CreateArticle(2, instrument, new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateArticle(3, instrument, new DateTime(2026, 4, 2, 15, 0, 0, DateTimeKind.Utc))
        );
        await context.SaveChangesAsync();

        var handler = new GetNewsArticlesHandler(context);
        var query = new GetNewsArticlesQuery
        {
            FromDate = new DateOnly(2026, 4, 2)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_NoDateFilters_ReturnsAllArticles()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var instrument = new Instrument { Id = 1, Name = "Test Co", Ticker = "TST", CurrencyId = "GBP" };
        context.Instruments.Add(instrument);
        context.NewsArticles.AddRange(
            CreateArticle(1, instrument, new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateArticle(2, instrument, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)),
            CreateArticle(3, instrument, new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc))
        );
        await context.SaveChangesAsync();

        var handler = new GetNewsArticlesHandler(context);
        var query = new GetNewsArticlesQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(3, result.TotalCount);
    }

    private static NewsArticle CreateArticle(int id, Instrument instrument, DateTime publishedUtc)
    {
        return new NewsArticle
        {
            Id = id,
            InstrumentId = instrument.Id,
            Instrument = instrument,
            PublishedUtc = publishedUtc,
            RetrievedUtc = publishedUtc.AddMinutes(5),
            Source = "Test",
            SourceArticleId = $"test-{id}",
            Headline = $"Test Article {id}",
            BodyHtml = "<p>Test</p>",
            BodyText = "Test",
            Url = $"https://example.com/{id}"
        };
    }

}
