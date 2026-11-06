using Clarion;

namespace Hoard.Core.Infrastructure.Integrations;

/// <summary>
/// Only compiled when <c>UseClarion</c> is <c>true</c>; Clarion is a private package.
/// </summary>
public class ClarionNewsService : INewsService
{
    private readonly IClarionClient _clarionClient = ClarionClient.Create();

    public async Task<IReadOnlyList<SourceArticleSummaryDto>> GetArticleSummariesAsync(string ticker, CancellationToken ct = default)
    {
        var summaries = await _clarionClient.GetArticlesAsync(ticker, ct);

        return summaries
            .Select(s => new SourceArticleSummaryDto
            {
                Source = s.Source,
                SourceArticleId = s.SourceArticleId,
                Headline = s.Headline,
                PublishedUtc = s.PublishedUtc,
                Url = s.Url
            })
            .ToList();
    }

    public async Task<SourceArticleDto> GetArticleAsync(string sourceArticleId, CancellationToken ct = default)
    {
        var article = await _clarionClient.GetArticleAsync(sourceArticleId, ct);

        return new SourceArticleDto
        {
            Source = article.Source,
            SourceArticleId = article.SourceArticleId,
            Headline = article.Headline,
            Url = article.Url,
            BodyHtml = article.BodyHtml,
            BodyText = article.BodyText
        };
    }
}
