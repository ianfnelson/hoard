namespace Hoard.Core.Messages;

/// <summary>
/// Recalculate all holdings for all accounts, for the specified day.
/// This will be scheduled to run in the early hours of every day.
/// </summary>
/// <param name="AsOfDate">Date for which to recalculate holdings.</param>
public record RecalculateHoldingsCommand(DateOnly AsOfDate);

/// <summary>
/// Event raised when holdings have been recalculated.
/// </summary>
/// <param name="AsOfDate">Date for which holdings have been recalculated.</param>
public record HoldingsRecalculatedEvent(DateOnly AsOfDate);