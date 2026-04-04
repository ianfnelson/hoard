using Clarion;
using Clarion.Models;
using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.News;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.News;

public class ProcessRefreshNewsBatchHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IClarionClient> _clarionClient = new();
    private readonly ProcessRefreshNewsBatchHandler _handler;

    public ProcessRefreshNewsBatchHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessRefreshNewsBatchHandler(
            _bus.Object,
            _context,
            _clarionClient.Object,
            NullLogger<ProcessRefreshNewsBatchHandler>.Instance);
    }

    private async Task<Instrument> AddInstrumentWithNewsFeed(int id = 1, string ticker = "TST")
    {
        var instrument = new Instrument
        {
            Id = id, Name = "Test Stock", Ticker = "TST", CurrencyId = Currency.Gbp,
            InstrumentTypeId = 1, AssetSubclassId = 1
        };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = id, DataFeedId = DataFeed.News,
            Configuration = new DataFeedConfiguration { Ticker = ticker },
            CreatedUtc = DateTime.UtcNow
        });
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();
        return instrument;
    }

    [Fact]
    public async Task HandleAsync_UnknownInstrument_ReturnsEarly()
    {
        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 999));

        _clarionClient.Verify(c => c.GetArticlesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoNewsFeed_ReturnsEarly()
    {
        var instrument = TestData.Instrument(id: 10);
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, instrument.Id));

        _clarionClient.Verify(c => c.GetArticlesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PublishesNewsRefreshedEvent()
    {
        await AddInstrumentWithNewsFeed();

        _clarionClient.Setup(c => c.GetArticlesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleSummary>());

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(runId, PipelineMode.CloseOfDay, 1));

        _bus.Verify(b => b.Publish(It.Is<NewsRefreshedEvent>(e =>
            e.NewsRunId == runId && e.InstrumentId == 1), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SkipsExistingArticles()
    {
        await AddInstrumentWithNewsFeed();

        _context.NewsArticles.Add(new NewsArticle
        {
            InstrumentId = 1, Source = "Investegate", SourceArticleId = "ABC123",
            PublishedUtc = DateTime.UtcNow, RetrievedUtc = DateTime.UtcNow,
            Headline = "Old", BodyHtml = "", BodyText = "", Url = "http://example.com"
        });
        await _context.SaveChangesAsync();

        _clarionClient.Setup(c => c.GetArticlesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleSummary>
            {
                new() { Source = "Investegate", SourceArticleId = "ABC123", PublishedUtc = DateTime.UtcNow, Ticker = "TST", Headline = "Old", Url = "http://example.com" }
            });

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 1));

        _clarionClient.Verify(c => c.GetArticleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var articles = await _context.NewsArticles.ToListAsync();
        Assert.Single(articles);
    }

    [Fact]
    public async Task HandleAsync_CreatesNewArticlesAndPublishesEvents()
    {
        await AddInstrumentWithNewsFeed();

        var publishedUtc = DateTime.UtcNow;

        _clarionClient.Setup(c => c.GetArticlesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleSummary>
            {
                new() { Source = "Investegate", SourceArticleId = "NEW001", PublishedUtc = publishedUtc, Ticker = "TST", Headline = "Breaking news", Url = "http://example.com/article" }
            });

        _clarionClient.Setup(c => c.GetArticleAsync("NEW001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Article
            {
                Source = "Investegate", SourceArticleId = "NEW001",
                Headline = "Breaking news", BodyHtml = "<p>Body</p>", BodyText = "Body",
                Url = "http://example.com/article", RetrievedUtc = DateTime.UtcNow
            });

        var runId = Guid.NewGuid();
        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(runId, PipelineMode.DaytimeReactive, 1));

        var article = await _context.NewsArticles.SingleAsync();
        Assert.Equal("Breaking news", article.Headline);
        Assert.Equal("Investegate", article.Source);

        _bus.Verify(b => b.Publish(It.Is<NewsArticleCreatedEvent>(e =>
            e.NewsRunId == runId && e.InstrumentId == 1 && e.Headline == "Breaking news"), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ContinuesOnArticleFetchError()
    {
        await AddInstrumentWithNewsFeed();

        _clarionClient.Setup(c => c.GetArticlesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleSummary>
            {
                new() { Source = "Investegate", SourceArticleId = "FAIL01", PublishedUtc = DateTime.UtcNow, Ticker = "TST", Headline = "Will fail", Url = "http://example.com/fail" },
                new() { Source = "Investegate", SourceArticleId = "OK01", PublishedUtc = DateTime.UtcNow, Ticker = "TST", Headline = "Will succeed", Url = "http://example.com/ok" }
            });

        _clarionClient.Setup(c => c.GetArticleAsync("FAIL01", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        _clarionClient.Setup(c => c.GetArticleAsync("OK01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Article
            {
                Source = "Investegate", SourceArticleId = "OK01",
                Headline = "Will succeed", BodyHtml = "<p>OK</p>", BodyText = "OK",
                Url = "http://example.com/ok", RetrievedUtc = DateTime.UtcNow
            });

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, 1));

        // Should still create the second article despite first failing
        var articles = await _context.NewsArticles.ToListAsync();
        Assert.Single(articles);
        Assert.Equal("Will succeed", articles[0].Headline);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
