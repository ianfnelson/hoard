namespace Hoard.Core.Messages.Prices;

/// <summary>
/// Initiates backfill of historical prices for an instrument.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
public record BackfillPricesCommand(Guid CorrelationId)
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
