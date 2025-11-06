namespace Hoard.Core.Messages.Holdings;

/// <summary>
/// Calculate all holdings for all accounts, for the specified day.
/// This will be scheduled to run in the early hours of every day.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
public record CalculateHoldingsCommand(Guid CorrelationId)
{
    /// <summary>
    /// Date for which holdings to be calculated.
    /// If not specified, defaults to today.
    /// </summary>
    public DateOnly? AsOfDate { get; init; }
}