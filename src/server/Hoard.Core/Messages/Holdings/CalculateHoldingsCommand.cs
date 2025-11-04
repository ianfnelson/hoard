namespace Hoard.Core.Messages.Holdings;

/// <summary>
/// Calculate all holdings for all accounts, for the specified day.
/// This will be scheduled to run in the early hours of every day.
/// </summary>
public record CalculateHoldingsCommand
{
    /// <summary>
    /// Date for which holdings to be calculated.
    /// If not specified, defaults to today.
    /// </summary>
    public DateOnly? AsOfDate { get; init; }
}

/// <summary>
/// Event raised when holdings have been calculated.
/// </summary>
/// <param name="AsOfDate">Date for which holdings have been calculated.</param>
public record HoldingsCalculatedEvent(DateOnly AsOfDate);