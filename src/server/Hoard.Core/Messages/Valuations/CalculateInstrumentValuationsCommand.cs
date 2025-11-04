namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Calculate valuations of instruments at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to calculate valuation</param>
/// <param name="InstrumentIds">Optional list of instrument IDs to be valued</param>
public record CalculateInstrumentValuationsCommand(DateOnly AsOfDate, IReadOnlyCollection<int>? InstrumentIds = null);