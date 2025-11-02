namespace Hoard.Core.Messages;

/// <summary>
/// Fetch daily prices for instruments.
/// This will be scheduled to run after the close of the trading day.
/// </summary>
/// <param name="AsOfDate">Date for which to fetch daily prices</param>
public record FetchDailyPricesCommand(DateOnly AsOfDate);

/// <summary>
/// Event published when price has been updated for an instrument.
/// </summary>
/// <param name="InstrumentId">ID of instrument whose price has been updated</param>
/// <param name="AsOfDate">Date to which the price relates</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record PriceUpdatedEvent(int InstrumentId, DateOnly AsOfDate, DateTime RetrievedUtc);