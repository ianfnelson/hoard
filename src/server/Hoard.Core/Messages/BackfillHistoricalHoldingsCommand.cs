namespace Hoard.Core.Messages;

/// <summary>
/// Initiates backfill of historical holdings for a date range and account
/// </summary>
/// <param name="AccountId">ID for which to backfill holdings</param>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record BackfillHistoricalHoldingsCommand(int AccountId, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Internal saga step message during historical valuation recalculation
/// </summary>
/// <param name="AccountId">ID for which to backfill holdings</param>
/// <param name="AsOfDate">Date for which to recalculate valuations</param>
public record RecalculateHistoricalHoldingsCommand(int AccountId, DateOnly AsOfDate);