namespace Hoard.Messages.Prices;

/// <summary>
/// Event published when price has been refreshed for an instrument.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="InstrumentId">ID of instrument whose price has been refreshed</param>
/// <param name="StartDate">StartDate of range for which prices refreshed</param>
/// <param name="EndDate">EndDate of range for which prices refreshed</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record PriceRefreshedEvent(Guid CorrelationId, int InstrumentId, DateOnly StartDate, DateOnly EndDate, DateTime RetrievedUtc);