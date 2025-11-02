namespace Hoard.Core.Messages;

/// <summary>
/// Initiates backfill of historical holdings for a date range and account
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record BackfillHistoricalHoldingsCommand(Guid BatchId, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Internal saga step message during historical valuation recalculation
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="AsOfDate">Date for which to recalculate holdings</param>
public record RecalculateHistoricalHoldingsCommand(Guid BatchId, DateOnly AsOfDate);

/// <summary>
/// Event raised when historical holdings have been recalculated for a date
/// </summary>
/// <<param name="BatchId">For correlating messages</param>
/// <param name="AsOfDate">Date for which holdings have been recalculated</param>
public record HistoricalHoldingsRecalculatedEvent(Guid BatchId, DateOnly AsOfDate);