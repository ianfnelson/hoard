namespace Hoard.Messages.Quotes;

/// <summary>
/// Refresh quotes for the specified instruments.
/// Raised by <see cref="RefreshQuotesBatchBusCommand"/>
/// </summary>
/// <param name="InstrumentIds"></param>
/// <param name="CorrelationId">For correlation purposes</param>
public record RefreshQuotesBatchBusCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds);