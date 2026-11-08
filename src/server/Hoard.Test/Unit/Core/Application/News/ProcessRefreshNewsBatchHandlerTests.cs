using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
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
    private readonly Mock<INewsService> _newsService = new();
    private readonly ProcessRefreshNewsBatchHandler _handler;

    public ProcessRefreshNewsBatchHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessRefreshNewsBatchHandler(
            _bus.Object,
            _context,
            _newsService.Object,
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

        _newsService.Verify(c => c.GetArticleSummariesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoNewsFeed_ReturnsEarly()
    {
        var instrument = TestData.Instrument(id: 10);
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, instrument.Id));

        _newsService.Verify(c => c.GetArticleSummariesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PublishesNewsRefreshedEvent()
    {
        await AddInstrumentWithNewsFeed();

        _newsService.Setup(c => c.GetArticleSummariesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SourceArticleSummaryDto>());

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

        _newsService.Setup(c => c.GetArticleSummariesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SourceArticleSummaryDto>
            {
                new() { Source = "Investegate", SourceArticleId = "ABC123", PublishedUtc = DateTime.UtcNow, Headline = "Old", Url = "http://example.com" }
            });

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 1));

        _newsService.Verify(c => c.GetArticleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var articles = await _context.NewsArticles.ToListAsync();
        Assert.Single(articles);
    }

    [Fact]
    public async Task HandleAsync_CreatesNewArticlesAndPublishesEvents()
    {
        await AddInstrumentWithNewsFeed();

        var publishedUtc = DateTime.UtcNow;

        _newsService.Setup(c => c.GetArticleSummariesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SourceArticleSummaryDto>
            {
                new() { Source = "Investegate", SourceArticleId = "NEW001", PublishedUtc = publishedUtc, Headline = "Breaking news", Url = "http://example.com/article" }
            });

        _newsService.Setup(c => c.GetArticleAsync("NEW001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SourceArticleDto
            {
                Source = "Investegate", SourceArticleId = "NEW001",
                Headline = "Breaking news", BodyHtml = "<p>Body</p>", BodyText = "Body",
                Url = "http://example.com/article"
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

        _newsService.Setup(c => c.GetArticleSummariesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SourceArticleSummaryDto>
            {
                new() { Source = "Investegate", SourceArticleId = "FAIL01", PublishedUtc = DateTime.UtcNow, Headline = "Will fail", Url = "http://example.com/fail" },
                new() { Source = "Investegate", SourceArticleId = "OK01", PublishedUtc = DateTime.UtcNow, Headline = "Will succeed", Url = "http://example.com/ok" }
            });

        _newsService.Setup(c => c.GetArticleAsync("FAIL01", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        _newsService.Setup(c => c.GetArticleAsync("OK01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SourceArticleDto
            {
                Source = "Investegate", SourceArticleId = "OK01",
                Headline = "Will succeed", BodyHtml = "<p>OK</p>", BodyText = "OK",
                Url = "http://example.com/ok"
            });

        await _handler.HandleAsync(new ProcessRefreshNewsBatchCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, 1));

        // Should still create the second article despite first failing
        var articles = await _context.NewsArticles.ToListAsync();
        Assert.Single(articles);
        Assert.Equal("Will succeed", articles[0].Headline);
    }

    [Fact]
    public async Task HandleAsync_Cancelled_StopsAndDoesNotPublishNewsRefreshedEvent()
    {
        await AddInstrumentWithNewsFeed();
        using var cts = new CancellationTokenSource();

        _newsService.Setup(c => c.GetArticleSummariesAsync("TST", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SourceArticleSummaryDto>
            {
                new() { Source = "Investegate", SourceArticleId = "CXL01", PublishedUtc = DateTime.UtcNow, Headline = "Cancelled", Url = "http://example.com/1" },
                new() { Source = "Investegate", SourceArticleId = "CXL02", PublishedUtc = DateTime.UtcNow, Headline = "Never fetched", Url = "http://example.com/2" }
            });

        _newsService.Setup(c => c.GetArticleAsync("CXL01", It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((_, token) =>
            {
                cts.Cancel();
                return Task.FromException<SourceArticleDto>(new OperationCanceledException(token));
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _handler.HandleAsync(
            new ProcessRefreshNewsBatchCommand(Guid.NewGuid(), PipelineMode.CloseOfDay, 1), cts.Token));

        _newsService.Verify(c => c.GetArticleAsync("CXL02", It.IsAny<CancellationToken>()), Times.Never);
        _bus.Verify(b => b.Publish(It.IsAny<NewsRefreshedEvent>(), null), Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
