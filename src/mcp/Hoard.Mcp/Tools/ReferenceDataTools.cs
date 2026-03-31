using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class ReferenceDataTools
{
    [McpServerTool, Description("Get all asset classes (e.g. Equities, Fixed Income, Cash). These IDs are used when filtering instruments.")]
    public static async Task<string> GetAssetClasses(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/reference/asset-classes", ct);

    [McpServerTool, Description("Get all currencies. Used to understand instrument pricing currencies.")]
    public static async Task<string> GetCurrencies(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/reference/currencies", ct);

    [McpServerTool, Description("Get all instrument types (e.g. Share, Fund, ETF, Investment Trust). These IDs are used when filtering instruments.")]
    public static async Task<string> GetInstrumentTypes(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/reference/instrument-types", ct);

    [McpServerTool, Description("Get all transaction types (e.g. Buy, Sell, Dividend, Deposit). These IDs are required when creating transactions.")]
    public static async Task<string> GetTransactionTypes(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/reference/transaction-types", ct);
}
