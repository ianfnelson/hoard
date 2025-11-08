namespace Hoard.Messages.Valuations;

/// <summary>
/// Initiates backfill of valuations for a date range.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
public record StartBackfillValuationsSagaCommand(Guid CorrelationId)
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
