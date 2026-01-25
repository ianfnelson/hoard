using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.News;

public record GetNewsArticleQuery(int NewsArticleId) : IQuery<NewsArticleDetailDto?>;

public sealed class GetNewsArticleHandler(HoardContext context, ILogger<GetNewsArticleHandler> logger)
    : IQueryHandler<GetNewsArticleQuery, NewsArticleDetailDto?>
{
    public async Task<NewsArticleDetailDto?> HandleAsync(GetNewsArticleQuery query, CancellationToken ct = default)
    {
        var dto = await context.NewsArticles
            .AsNoTracking()
            .Where(n => n.Id == query.NewsArticleId)
            .Select(n => new NewsArticleDetailDto
            {
                Id = n.Id,
                PublishedUtc = n.PublishedUtc,
                RetrievedUtc = n.RetrievedUtc,
                Headline = n.Headline,
                BodyHtml = n.BodyHtml,
                Url = n.Url,
                InstrumentId = n.InstrumentId,
                InstrumentName = n.Instrument.Name,
                InstrumentTicker = n.Instrument.TickerDisplay
            }).SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "News Article with id {NewsArticleId} not found",
                query.NewsArticleId);
        }

        return dto;
    }
}