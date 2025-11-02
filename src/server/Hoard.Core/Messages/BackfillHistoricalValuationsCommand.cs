namespace Hoard.Core.Messages;

/// <summary>
/// Initiates backfill of historical values for a date range.
/// </summary>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record BackfillHistoricalValuationsCommand(DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Internal saga step message during historical valuation recalculation
/// </summary>
/// <param name="AsOfDate">Date for which to recalculate valuations</param>
public record RecalculateHistoricalValuationCommand(DateOnly AsOfDate);
