using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class InstrumentTools
{
    [McpServerTool, Description("Search and list instruments (stocks, funds, ETFs, etc.). Results are paged.")]
    public static async Task<string> GetInstruments(
        HoardApiClient api,
        [Description("Search by name or ticker")] string? search,
        [Description("Filter by instrument type ID (use GetInstrumentTypes for valid IDs)")] int? instrumentTypeId,
        [Description("Filter by asset class ID (use GetAssetClasses for valid IDs)")] int? assetClassId,
        [Description("Filter by asset subclass ID")] int? assetSubclassId,
        [Description("Page number (default 1)")] int? pageNumber,
        [Description("Page size (default 10)")] int? pageSize,
        CancellationToken ct) =>
        await api.GetAsync("api/instruments" + HoardApiClient.BuildQuery(
            ("Search", search), ("InstrumentTypeId", instrumentTypeId), ("AssetClassId", assetClassId),
            ("AssetSubclassId", assetSubclassId), ("PageNumber", pageNumber), ("PageSize", pageSize)), ct);

    [McpServerTool, Description("Get detailed information about a specific instrument, including its type, asset class, currency, latest quote, and data feed configuration.")]
    public static async Task<string> GetInstrument(
        HoardApiClient api,
        [Description("The instrument ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/instruments/{id}", ct);

    [McpServerTool, Description("Get a simple lookup list of all instruments (ID and name only). Useful for finding instrument IDs.")]
    public static async Task<string> GetInstrumentLookup(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/instruments/lookup", ct);

    [McpServerTool, Description("Get paginated price history for an instrument, showing OHLCV data.")]
    public static async Task<string> GetInstrumentPrices(
        HoardApiClient api,
        [Description("The instrument ID")] int id,
        [Description("Page number (default 1)")] int? pageNumber,
        [Description("Page size (default 10)")] int? pageSize,
        CancellationToken ct) =>
        await api.GetAsync($"api/instruments/{id}/prices" + HoardApiClient.BuildQuery(("pageNumber", pageNumber), ("pageSize", pageSize)), ct);

    [McpServerTool, Description("Get a price time series for an instrument over a date range, suitable for charting.")]
    public static async Task<string> GetInstrumentPriceSeries(
        HoardApiClient api,
        [Description("The instrument ID")] int id,
        [Description("Start date in yyyy-MM-dd format")] string? from,
        [Description("End date in yyyy-MM-dd format")] string? to,
        CancellationToken ct) =>
        await api.GetAsync($"api/instruments/{id}/price-series" + HoardApiClient.BuildQuery(("from", from), ("to", to)), ct);

    [McpServerTool, Description("Get a valuation time series for an instrument over a date range, showing how its total value has changed.")]
    public static async Task<string> GetInstrumentValuationSeries(
        HoardApiClient api,
        [Description("The instrument ID")] int id,
        [Description("Start date in yyyy-MM-dd format")] string? from,
        [Description("End date in yyyy-MM-dd format")] string? to,
        CancellationToken ct) =>
        await api.GetAsync($"api/instruments/{id}/valuation-series" + HoardApiClient.BuildQuery(("from", from), ("to", to)), ct);
}
