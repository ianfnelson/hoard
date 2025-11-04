namespace Hoard.Core.Messages.Quotes;

/// <summary>
/// Refresh quotes for the specified instruments.
/// Raised by <see cref="RefreshQuotesBatchCommand"/>
/// </summary>
/// <param name="InstrumentIds"></param>
public record RefreshQuotesBatchCommand(IReadOnlyList<int> InstrumentIds);

/// <summary>
/// Event published when quote has been refreshed for an instrument.
/// </summary>
/// <param name="InstrumentId">ID of instrument whose quote has been refreshed.</param>
/// <param name="RetrievedUtc">DateTime in UTC at which the price was retrieved</param>
public record QuoteRefreshedEvent(int InstrumentId, DateTime RetrievedUtc);