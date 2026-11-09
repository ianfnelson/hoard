using Clarion;

namespace Hoard.Core.Infrastructure.Integrations;

public interface INewsService
{
    Task<IReadOnlyList<SourceArticleSummaryDto>> GetArticleSummariesAsync(string ticker, CancellationToken ct = default);

    Task<SourceArticleDto> GetArticleAsync(string sourceArticleId, CancellationToken ct = default);
}

public class SourceArticleSummaryDto
{
    public required string Source { get; set; }
    public required string SourceArticleId { get; set; }
    public required string Headline { get; set; }
    public DateTime PublishedUtc { get; set; }
    public required string Url { get; set; }
}

public class SourceArticleDto
{
    public required string Source { get; set; }
    public required string SourceArticleId { get; set; }
    public required string Headline { get; set; }
    public required string Url { get; set; }
    public required string BodyHtml { get; set; }
    public required string BodyText { get; set; }
}

public class ClarionNewsService(IClarionClient clarionClient) : INewsService
{
    public async Task<IReadOnlyList<SourceArticleSummaryDto>> GetArticleSummariesAsync(string ticker, CancellationToken ct = default)
    {
        var summaries = await clarionClient.GetArticlesAsync(ticker, ct);

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
        var article = await clarionClient.GetArticleAsync(sourceArticleId, ct);

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
