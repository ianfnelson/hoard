using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class PortfolioTools
{
    [McpServerTool, Description("Get a list of all portfolios with summary information including name, active status, and creation date.")]
    public static async Task<string> GetPortfolios(HoardApiClient api, CancellationToken ct) =>
        await api.GetAsync("api/portfolios", ct);

    [McpServerTool, Description("Get detailed information about a specific portfolio, including performance metrics.")]
    public static async Task<string> GetPortfolio(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}", ct);

    [McpServerTool, Description("Get the current positions (holdings) for a portfolio. Each position shows the instrument, open/close dates, performance, and portfolio weighting.")]
    public static async Task<string> GetPortfolioPositions(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        [Description("Filter by open/closed positions. True for open only, false for closed only, omit for all.")] bool? isOpen,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/positions" + HoardApiClient.BuildQuery(("isOpen", isOpen)), ct);

    [McpServerTool, Description("Get asset class and subclass exposure breakdown for a portfolio, including rebalance actions.")]
    public static async Task<string> GetPortfolioExposure(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/exposure", ct);

    [McpServerTool, Description("Get yearly snapshots for a portfolio showing annual returns, values, buys, sells, income, fees, and other metrics.")]
    public static async Task<string> GetPortfolioSnapshots(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/snapshots", ct);

    [McpServerTool, Description("Get the valuation of a portfolio, showing each instrument's value, units, and weighting. Optionally compare two dates.")]
    public static async Task<string> GetPortfolioValuation(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        [Description("Start date for comparison in yyyy-MM-dd format. Omit for current valuation only.")] string? from,
        [Description("End date in yyyy-MM-dd format. Defaults to today.")] string? to,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/valuation" + HoardApiClient.BuildQuery(("from", from), ("to", to)), ct);

    [McpServerTool, Description("Get a time series of portfolio valuations over a date range, useful for charting portfolio value over time.")]
    public static async Task<string> GetPortfolioValuationSeries(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        [Description("Start date in yyyy-MM-dd format")] string? from,
        [Description("End date in yyyy-MM-dd format")] string? to,
        [Description("Disaggregate the series by a dimension, e.g. 'instrument', 'assetClass', 'assetSubclass'")] string? disaggregateBy,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/valuation-series" + HoardApiClient.BuildQuery(("from", from), ("to", to), ("disaggregateBy", disaggregateBy)), ct);

    [McpServerTool, Description("Get a breakdown of instrument types held in a portfolio (e.g. shares, funds, ETFs) with counts and values.")]
    public static async Task<string> GetPortfolioInstrumentTypes(
        HoardApiClient api,
        [Description("The portfolio ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/portfolios/{id}/instrument-types", ct);
}
