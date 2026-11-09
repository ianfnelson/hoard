using Clarion;
using Clarion.Models;
using Hoard.Core.Infrastructure.Integrations;
using Moq;

namespace Hoard.Test.Unit.Core.Services.News;

public class ClarionNewsServiceTests
{
    private readonly Mock<IClarionClient> _clarionClient = new();
    private readonly ClarionNewsService _service;

    public ClarionNewsServiceTests()
    {
        _service = new ClarionNewsService(_clarionClient.Object);
    }

    [Fact]
    public async Task GetArticleSummariesAsync_MapsClarionSummaries()
    {
        var publishedUtc = new DateTime(2026, 11, 6, 7, 0, 0, DateTimeKind.Utc);
        _clarionClient.Setup(c => c.GetArticlesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleSummary>
            {
                new() { Source = "Investegate", SourceArticleId = "A1", PublishedUtc = publishedUtc, Ticker = "TST", Headline = "Results", Url = "http://example.com/a1" }
            });

        var summary = Assert.Single(await _service.GetArticleSummariesAsync("TST"));

        Assert.Equal("Investegate", summary.Source);
        Assert.Equal("A1", summary.SourceArticleId);
        Assert.Equal("Results", summary.Headline);
        Assert.Equal(publishedUtc, summary.PublishedUtc);
        Assert.Equal(DateTimeKind.Utc, summary.PublishedUtc.Kind);
        Assert.Equal("http://example.com/a1", summary.Url);
    }

    [Fact]
    public async Task GetArticleAsync_MapsClarionArticle()
    {
        _clarionClient.Setup(c => c.GetArticleAsync("A1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Article
            {
                Source = "Investegate", SourceArticleId = "A1", Headline = "Results",
                BodyHtml = "<p>Body</p>", BodyText = "Body", Url = "http://example.com/a1",
                RetrievedUtc = DateTime.UtcNow
            });

        var article = await _service.GetArticleAsync("A1");

        Assert.Equal("Investegate", article.Source);
        Assert.Equal("A1", article.SourceArticleId);
        Assert.Equal("Results", article.Headline);
        Assert.Equal("<p>Body</p>", article.BodyHtml);
        Assert.Equal("Body", article.BodyText);
        Assert.Equal("http://example.com/a1", article.Url);
    }
}
