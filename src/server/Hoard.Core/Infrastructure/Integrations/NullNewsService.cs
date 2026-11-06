namespace Hoard.Core.Infrastructure.Integrations;

/// <summary>
/// Registered when the build excludes Clarion (<c>UseClarion</c> is not <c>true</c>), so news refreshes
/// complete normally without fetching any articles.
/// </summary>
public class NullNewsService : INewsService
{
    public Task<IReadOnlyList<SourceArticleSummaryDto>> GetArticleSummariesAsync(string ticker, CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<SourceArticleSummaryDto>>([]);
    }

    public Task<SourceArticleDto> GetArticleAsync(string sourceArticleId, CancellationToken ct = default)
    {
        throw new InvalidOperationException(
            $"Cannot fetch article {sourceArticleId}: news is disabled because this build excludes Clarion.");
    }
}
