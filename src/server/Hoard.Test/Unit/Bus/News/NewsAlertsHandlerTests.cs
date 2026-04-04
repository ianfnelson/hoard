using Hoard.Bus.News;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages;
using Hoard.Messages.News;
using Moq;

namespace Hoard.Test.Unit.Bus.News;

public class NewsAlertsHandlerTests
{
    [Fact]
    public async Task Handle_DispatchesNewsArticleAlertCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new NewsAlertsHandler(mediator.Object);
        var publishedUtc = DateTime.UtcNow;

        await handler.Handle(new NewsArticleCreatedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 42, 100, publishedUtc, "Breaking News"));

        mediator.Verify(m => m.SendAsync(
            It.Is<NewsArticleAlertCommand>(c =>
                c.PipelineMode == PipelineMode.DaytimeReactive &&
                c.NewsArticleId == 100 &&
                c.PublishedUtc == publishedUtc &&
                c.Headline == "Breaking News"),
            default), Times.Once);
    }
}
