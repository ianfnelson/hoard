using Clarion;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.News;

public record ProcessRefreshNewsBatchCommand(
    Guid NewsRunId,
    PipelineMode PipelineMode,
    int InstrumentId) : ICommand;

public class ProcessRefreshNewsBatchHandler(
    IBus bus,
    HoardContext context,
    ClarionClient clarionClient,
    ILogger<ProcessRefreshNewsBatchHandler> logger)
    : ICommandHandler<ProcessRefreshNewsBatchCommand>
{
    public async Task HandleAsync(ProcessRefreshNewsBatchCommand command, CancellationToken ct = default)
    {
        var instrument = await context.Instruments
            .FirstOrDefaultAsync(i => i.Id == command.InstrumentId, ct);

        if (instrument == null)
        {
            logger.LogWarning("Received RefreshNewsBatchCommand with unknown Instrument {InstrumentId}",
                command.InstrumentId);
            return;
        }

        if (instrument.TickerNewsUpdates == null || !instrument.EnableNewsUpdates)
        {
            logger.LogWarning("News updates not possible for Instrument {InstrumentId}",
                command.InstrumentId);
            return;
        }

        var now = DateTime.UtcNow;

        var articleSummaries = await GetArticleSummariesAsync(instrument, ct);
        var existingSourceIds = await GetExistingArticleKeysAsync(command.InstrumentId, ct);
        var newSummaries = FilterNewSummaries(articleSummaries, existingSourceIds);

        logger.LogDebug(
            "Found {TotalCount} articles for {Ticker}, {NewCount} are new",
            articleSummaries.Count,
            instrument.TickerNewsUpdates,
            newSummaries.Count);

        await FetchAndCreateNewArticlesAsync(newSummaries, instrument.Id, now, ct);

        await bus.Publish(new NewsRefreshedEvent(
            command.NewsRunId,
            command.PipelineMode,
            instrument.Id,
            now));

        logger.LogInformation(
            "News refreshed for Instrument {InstrumentId}, inserted {Count} new articles",
            instrument.Id,
            newSummaries.Count);
    }

    private async Task<IReadOnlyList<Clarion.Models.ArticleSummary>> GetArticleSummariesAsync(Instrument instrument, CancellationToken ct)
    {
        var articleSummaries = await clarionClient.GetArticlesAsync(instrument.TickerNewsUpdates!, ct);

        if (instrument.NewsImportStartUtc.HasValue)
        {
            articleSummaries = articleSummaries
                .Where(a => a.PublishedUtc >= instrument.NewsImportStartUtc.Value)
                .ToList();
        }

        return articleSummaries;
    }

    private async Task<HashSet<string>> GetExistingArticleKeysAsync(int instrumentId, CancellationToken ct)
    {
        return await context.NewsArticles
            .Where(na => na.InstrumentId == instrumentId)
            .Select(na => na.Source + "|" + na.SourceArticleId)
            .ToHashSetAsync(ct);
    }

    private static List<Clarion.Models.ArticleSummary> FilterNewSummaries(
        IReadOnlyList<Clarion.Models.ArticleSummary> articleSummaries,
        HashSet<string> existingSourceIds)
    {
        return articleSummaries
            .Where(s => !existingSourceIds.Contains(s.Source + "|" + s.SourceArticleId))
            .ToList();
    }

    private async Task FetchAndCreateNewArticlesAsync(
        List<Clarion.Models.ArticleSummary> newSummaries,
        int instrumentId,
        DateTime retrievedUtc,
        CancellationToken ct)
    {
        foreach (var summary in newSummaries)
        {
            try
            {
                if (newSummaries.IndexOf(summary) > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }

                var article = await clarionClient.GetArticleAsync(summary.SourceArticleId, ct);

                var newsArticle = new NewsArticle
                {
                    InstrumentId = instrumentId,
                    PublishedUtc = summary.PublishedUtc,
                    RetrievedUtc = retrievedUtc,
                    Source = article.Source,
                    SourceArticleId = article.SourceArticleId,
                    Headline = article.Headline,
                    BodyHtml = article.BodyHtml,
                    BodyText = article.BodyText,
                    Url = article.Url
                };

                context.Add(newsArticle);
                await context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to fetch article {SourceArticleId} for Instrument {InstrumentId}",
                    summary.SourceArticleId,
                    instrumentId);
            }
        }
    }
}
