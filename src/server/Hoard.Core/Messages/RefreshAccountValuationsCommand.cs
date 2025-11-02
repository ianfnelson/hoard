namespace Hoard.Core.Messages;

/// <summary>
/// Refresh valuations of accounts at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to refresh valuation</param>
/// <param name="AccountIds">Optional list of account IDs to be valued</param>
public record RefreshAccountValuationsCommand(DateOnly AsOfDate, IReadOnlyCollection<int>? AccountIds = null);