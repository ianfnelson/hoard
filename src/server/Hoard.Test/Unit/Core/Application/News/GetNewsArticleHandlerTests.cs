using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.News;

public class GetNewsArticleHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingArticle_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        context.NewsArticles.Add(new NewsArticle
        {
            Id = 1, InstrumentId = 1,
            PublishedUtc = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            RetrievedUtc = new DateTime(2024, 1, 15, 10, 5, 0, DateTimeKind.Utc),
            Source = "Investegate", SourceArticleId = "inv-001",
            Headline = "Test Headline", BodyHtml = "<p>Body</p>", BodyText = "Body",
            Url = "https://example.com/1"
        });
        await context.SaveChangesAsync();

        var handler = new GetNewsArticleHandler(context, NullLogger<GetNewsArticleHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetNewsArticleQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Headline", result.Headline);
        Assert.Equal("Test Stock", result.InstrumentName);
    }

    [Fact]
    public async Task HandleAsync_NonExistentArticle_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetNewsArticleHandler(context, NullLogger<GetNewsArticleHandler>.Instance);

        var result = await handler.HandleAsync(new GetNewsArticleQuery(999));

        Assert.Null(result);
    }
}
