namespace Hoard.Core.Messages.Quotes;

/// <summary>
/// Event published when quote has been refreshed for an instrument.
/// </summary>
/// <param name="CorrelationId">For correlation purposes</param>
/// <param name="InstrumentId">ID of instrument whose quote has been refreshed.</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record QuoteRefreshedEvent(Guid CorrelationId, int InstrumentId, DateTime RetrievedUtc);