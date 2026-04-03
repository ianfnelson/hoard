using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class AccountTools
{
    [McpServerTool, Description("List accounts. An account represents a real account held with a financial institution (e.g. an ISA, SIPP, or trading account). Transactions belong to accounts. Accounts can be grouped into portfolios for cross-account analysis and reporting. Returns id, name, colour, and active status.")]
    public static async Task<string> GetAccounts(
        HoardApiClient api,
        [Description("Filter by portfolio ID")] int? portfolioId,
        [Description("Filter by active status (true/false)")] bool? isActive,
        CancellationToken ct) =>
        await api.GetAsync("api/accounts" + HoardApiClient.BuildQuery(
            ("PortfolioId", portfolioId), ("IsActive", isActive)), ct);

    [McpServerTool, Description("Get detailed information about a specific account, including the portfolios it belongs to.")]
    public static async Task<string> GetAccount(
        HoardApiClient api,
        [Description("The account ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/accounts/{id}", ct);
}
