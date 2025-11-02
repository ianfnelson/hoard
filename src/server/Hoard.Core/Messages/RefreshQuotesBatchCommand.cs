namespace Hoard.Core.Messages;

/// <summary>
/// Refresh quotes for the specified instruments.
/// Raised by <see cref="RefreshQuotesBatchCommand"/>
/// </summary>
/// <param name="InstrumentIds"></param>
public record RefreshQuotesBatchCommand(IReadOnlyList<int> InstrumentIds);

/// <summary>
/// Event published when quote has been updated for an instrument.
/// </summary>
/// <param name="InstrumentId">ID of instrument whose quote has been updated.</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record QuoteUpdatedEvent(int InstrumentId, DateTime RetrievedUtc);