namespace Hoard.Core.Messages;

/// <summary>
/// Trigger the recalculation of all valuations, for the specified date.
/// </summary>
/// <param name="AsOfDate">Date for which to recalculate all valuations</param>
public record RecalculateValuationsCommand(DateOnly AsOfDate);