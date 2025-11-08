namespace Hoard.Messages.Prices;

/// <summary>
/// Refresh daily prices for a single instrument
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="InstrumentId">ID of instrument for which to retrieve prices</param>
/// <param name="StartDate">StartDate of range to be retrieved</param>
/// <param name="EndDate">EndDate of range to be retrieved</param>
public record RefreshPricesBatchBusCommand(Guid CorrelationId, int InstrumentId, DateOnly StartDate, DateOnly EndDate);