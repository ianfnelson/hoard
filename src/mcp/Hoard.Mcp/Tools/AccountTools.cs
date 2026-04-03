using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class AccountTools
{
    [McpServerTool, Description("List accounts. Each account holds portfolios and is referenced when creating transactions. Returns id, name, colour, and active status.")]
    public static async Task<string> GetAccounts(
        HoardApiClient api,
        [Description("Filter by portfolio ID")] int? portfolioId,
        [Description("Filter by active status (true/false)")] bool? isActive,
        CancellationToken ct) =>
        await api.GetAsync("api/accounts" + HoardApiClient.BuildQuery(
            ("PortfolioId", portfolioId), ("IsActive", isActive)), ct);

    [McpServerTool, Description("Get detailed information about a specific account, including its portfolios.")]
    public static async Task<string> GetAccount(
        HoardApiClient api,
        [Description("The account ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/accounts/{id}", ct);
}
