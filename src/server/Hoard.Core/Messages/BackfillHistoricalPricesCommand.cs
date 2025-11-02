namespace Hoard.Core.Messages;

/// <summary>
/// Initiates backfill of historical prices for an instrument.
/// </summary>
/// <param name="InstrumentId">Instrument for which prices to be backfilled.</param>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record BackfillHistoricalPricesCommand(int InstrumentId, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Event raised on completion of backfill of historical prices for an instrument.
/// </summary>
/// <param name="InstrumentId">Instrument for which prices have been backfilled.</param>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record HistoricalPricesBackfilledEvent(int InstrumentId, DateOnly StartDate, DateOnly EndDate);