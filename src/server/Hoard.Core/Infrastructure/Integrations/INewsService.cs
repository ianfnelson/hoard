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
