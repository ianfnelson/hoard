using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class NewsTools
{
    [McpServerTool, Description("Search and list regulatory news articles linked to instruments. Results are paged.")]
    public static async Task<string> GetNewsArticles(
        HoardApiClient api,
        [Description("Filter from date in yyyy-MM-dd format")] string? fromDate = null,
        [Description("Filter to date in yyyy-MM-dd format")] string? toDate = null,
        [Description("Filter by instrument ID")] int? instrumentId = null,
        [Description("Search text in headlines")] string? search = null,
        [Description("Page number (default 1)")] int? pageNumber = null,
        [Description("Page size (default 10)")] int? pageSize = null,
        CancellationToken ct = default) =>
        await api.GetAsync("api/news-articles" + HoardApiClient.BuildQuery(
            ("FromDate", fromDate), ("ToDate", toDate), ("InstrumentId", instrumentId),
            ("Search", search), ("PageNumber", pageNumber), ("PageSize", pageSize)), ct);

    [McpServerTool, Description("Get a specific news article with full body content.")]
    public static async Task<string> GetNewsArticle(
        HoardApiClient api,
        [Description("The news article ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/news-articles/{id}", ct);
}
