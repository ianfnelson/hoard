namespace Hoard.Core.Messages;

/// <summary>
/// Refresh valuations of instruments at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to refresh valuation</param>
/// <param name="InstrumentIds">Optional list of instrument IDs to be valued</param>
public record RefreshInstrumentValuationsCommand(DateOnly AsOfDate, IReadOnlyCollection<int>? InstrumentIds = null);