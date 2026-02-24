using Hoard.Core.Application.News;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record NewsArticleAlertCommand(PipelineMode PipelineMode, int NewsArticleId, DateTime PublishedUtc, string Headline)
    : ICommand;
    
public class NewsArticleAlertHandler(
    INewsAlertSuppressionRuleProvider suppressionRuleProvider,
    HoardContext hoardContext,
    IAlertService alertService) : ICommandHandler<NewsArticleAlertCommand>
{
    public async Task HandleAsync(NewsArticleAlertCommand command, CancellationToken ct = default)
    {
        if (command.PipelineMode != PipelineMode.DaytimeReactive)
        {
            return;
        }

        if (!command.PublishedUtc.Date.Equals(DateTime.UtcNow.Date))
        {
            return;
        }
        
        var headlineContains = suppressionRuleProvider.GetHeadlineContains();
        if (headlineContains.Any(x => command.Headline.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var alertRequest = await BuildAlertRequest(command, ct);
        await alertService.TryRaiseAsync(alertRequest, ct);
    }

    private async Task<AlertRequest> BuildAlertRequest(NewsArticleAlertCommand command, CancellationToken ct)
    {
        var newsArticle = await hoardContext.NewsArticles
            .AsNoTracking()
            .Include(x => x.Instrument)
            .SingleOrDefaultAsync(x => x.Id == command.NewsArticleId, ct);

        if (newsArticle is null)
        {
            throw new KeyNotFoundException($"News article with ID {command.NewsArticleId} not found");
        }
        
        return new AlertRequest
        {
            AlertTypeId = AlertType.NewsArticle,
            TriggeredDate = DateOnly.FromDateTime(command.PublishedUtc),
            InstrumentId = newsArticle.InstrumentId,
            SuppressionKey = $"{AlertType.NewsArticle}:{command.NewsArticleId}",
            Title = $"{newsArticle.Instrument.Name} — News",
            Message = newsArticle.Headline,
            Url = newsArticle.Url,
            UrlTitle = "Source Article"
        };
    }
} 