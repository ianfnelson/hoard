using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Application.News;

public interface INewsAlertSuppressionRuleProvider
{
    IReadOnlyList<string> GetHeadlineContains();
}

public class NewsAlertSuppressionRuleProvider(
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache) : INewsAlertSuppressionRuleProvider

{
    public IReadOnlyList<string> GetHeadlineContains()
    {
        return cache.GetOrCreate(CacheKeys.NewsAlertSuppressionRules, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HoardContext>();

            return db.NewsAlertSuppressionRules
                .AsNoTracking()
                .Select(x => x.HeadlineContains)
                .ToList();
        })!;
    }
}