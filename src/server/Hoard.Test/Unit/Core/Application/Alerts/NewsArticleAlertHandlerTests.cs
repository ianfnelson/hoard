using Hoard.Core;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Test.Shared;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class NewsArticleAlertHandlerTests : IDisposable
{
    private static readonly DateTime TodayUtc = new(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly Mock<INewsAlertSuppressionRuleProvider> _suppressionProvider = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly NewsArticleAlertHandler _handler;

    public NewsArticleAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _suppressionProvider.Setup(s => s.GetHeadlineContains()).Returns(new List<string>());
        _timeProvider.SetUtcNow(new DateTimeOffset(TodayUtc, TimeSpan.Zero));
        _handler = new NewsArticleAlertHandler(_suppressionProvider.Object, _context, _alertService.Object, _timeProvider);
    }

    [Fact]
    public async Task HandleAsync_NonDaytimeReactive_DoesNotRaiseAlert()
    {
        await _handler.HandleAsync(new NewsArticleAlertCommand(
            PipelineMode.CloseOfDay, 1, TodayUtc, "Headline"));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NotPublishedToday_DoesNotRaiseAlert()
    {
        var yesterday = TodayUtc.AddDays(-1);

        await _handler.HandleAsync(new NewsArticleAlertCommand(
            PipelineMode.DaytimeReactive, 1, yesterday, "Headline"));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SuppressedHeadline_DoesNotRaiseAlert()
    {
        _suppressionProvider.Setup(s => s.GetHeadlineContains()).Returns(new List<string> { "Total Voting Rights" });

        await _handler.HandleAsync(new NewsArticleAlertCommand(
            PipelineMode.DaytimeReactive, 1, TodayUtc, "Company Announces Total Voting Rights"));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidArticle_RaisesAlert()
    {
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);

        var article = new NewsArticle
        {
            InstrumentId = instrument.Id, Source = "Investegate", SourceArticleId = "A001",
            PublishedUtc = TodayUtc, RetrievedUtc = TodayUtc,
            Headline = "Breaking News", BodyHtml = "<p>Body</p>", BodyText = "Body",
            Url = "http://example.com/article"
        };
        _context.NewsArticles.Add(article);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new NewsArticleAlertCommand(
            PipelineMode.DaytimeReactive, article.Id, TodayUtc, "Breaking News"));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.NewsArticle &&
            r.InstrumentId == instrument.Id &&
            r.Message == "Breaking News"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ArticleNotFound_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.HandleAsync(new NewsArticleAlertCommand(
                PipelineMode.DaytimeReactive, 999, TodayUtc, "Missing")));
    }

    [Fact]
    public async Task HandleAsync_SuppressionIsCaseInsensitive()
    {
        _suppressionProvider.Setup(s => s.GetHeadlineContains()).Returns(new List<string> { "voting rights" });

        await _handler.HandleAsync(new NewsArticleAlertCommand(
            PipelineMode.DaytimeReactive, 1, TodayUtc, "TOTAL VOTING RIGHTS"));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
