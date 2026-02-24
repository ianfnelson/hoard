using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.News;
using Rebus.Handlers;

namespace Hoard.Bus.News;

public class NewsAlertsHandler(IMediator mediator)
    : IHandleMessages<NewsArticleCreatedEvent>
{
    public async Task Handle(NewsArticleCreatedEvent message)
    {
        await mediator.SendAsync(new NewsArticleAlertCommand(message.PipelineMode, message.NewsArticleId, message.PublishedUtc, message.Headline));
    }
}