namespace Hoard.Core.Messages.Prices;

/// <summary>
/// Fetch daily prices for active instruments and currencies.
/// This will be scheduled to run after the close of the trading day.
/// </summary>
public record FetchPricesCommand
{
    /// <summary>
    /// Date for which to fetch daily prices.
    /// If not specified, will default to today.
    /// </summary>
    public DateOnly? AsOfDate { get; init; }
}

/// <summary>
/// Fetch daily prices for a single instrument
/// </summary>
/// <param name="InstrumentId">ID of instrument for which to retrieve prices</param>
/// <param name="AsOfDate">Date for which to fetch daily prices</param>
public record FetchPriceCommand(int InstrumentId, DateOnly AsOfDate);

/// <summary>
/// Event published when price has been updated for an instrument.
/// </summary>
/// <param name="InstrumentId">ID of instrument whose price has been updated</param>
/// <param name="AsOfDate">Date to which the price relates</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record PriceUpdatedEvent(int InstrumentId, DateOnly AsOfDate, DateTime RetrievedUtc);