namespace Hoard.Core.Messages;

/// <summary>
/// Refresh valuations of portfolios at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to refresh valuation</param>
/// <param name="PortfolioIds">Optional list of portfolio IDs to be valued</param>
public record RefreshPortfolioValuationsCommand(DateOnly AsOfDate, IReadOnlyCollection<int>? PortfolioIds = null);