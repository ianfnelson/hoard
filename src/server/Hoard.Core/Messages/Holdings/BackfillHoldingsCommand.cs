namespace Hoard.Core.Messages.Holdings;

/// <summary>
/// Initiates backfill of holdings for a date range and account
/// </summary>
/// <param name="BatchId">For correlating messages</param>
public record BackfillHoldingsCommand(Guid BatchId)
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

/// <summary>
/// Internal saga step message during valuation calculation
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="AsOfDate">Date for which to calculate holdings</param>
public record BackfillHoldingsForDateCommand(Guid BatchId, DateOnly AsOfDate);

/// <summary>
/// Event raised when historical holdings have been calculated for a date
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="AsOfDate">Date for which holdings have been calculated</param>
public record HoldingsBackfilledForDateEvent(Guid BatchId, DateOnly AsOfDate);