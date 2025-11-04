namespace Hoard.Core.Messages.Prices;

/// <summary>
/// Initiates backfill of historical prices for an instrument.
/// </summary>
/// <param name="BatchId">For correlating messages</param>
public record BackfillPricesCommand(Guid BatchId)
{
    /// <summary>
    /// Instrument for which prices to be backfilled.
    /// If not specified, all active holdings (including currencies) will be backfilled.
    /// </summary>
    public int? InstrumentId { get; init; }
    
    /// <summary>
    /// Start date of backfill range.
    /// If not specified, will default to the earliest holding date for each instrument.
    /// </summary>
    public DateOnly? StartDate { get; init; }
    
    /// <summary>
    /// End date of backfill range.
    /// If not specified, will default to latest holding date for each instrument.
    /// </summary>
    public DateOnly? EndDate { get; init; }
}

/// <summary>
/// Internal saga step during prices backfill
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="InstrumentId">Instrument for which prices to be backfilled</param>
/// <param name="StartDate">Start date of range</param>
/// <param name="EndDate">End date of range</param>
public record BackfillPricesBatchCommand(Guid BatchId, int InstrumentId, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Event raised on completion of backfill of historical prices for an instrument.
/// </summary>
/// <param name="BatchId">For correlating messages</param>
/// <param name="InstrumentId">Instrument for which prices have been backfilled.</param>
/// <param name="StartDate">Start date of backfill range</param>
/// <param name="EndDate">End date of backfill range</param>
public record PricesBackfilledEvent(Guid BatchId, int InstrumentId, DateOnly StartDate, DateOnly EndDate);