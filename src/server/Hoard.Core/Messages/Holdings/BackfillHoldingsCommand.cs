namespace Hoard.Core.Messages.Holdings;

/// <summary>
/// Initiates backfill of holdings for a date range and account
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
public record BackfillHoldingsCommand(Guid CorrelationId)
{
    /// <summary>
    /// Start date of range for backfill.
    /// If null, will default to the earliest trade date.
    /// </summary>
    public DateOnly? StartDate { get; init; }
    
    /// <summary>
    /// End date of range for backfill.
    /// if null, will default to today.
    /// </summary>
    public DateOnly? EndDate { get; init; }
}