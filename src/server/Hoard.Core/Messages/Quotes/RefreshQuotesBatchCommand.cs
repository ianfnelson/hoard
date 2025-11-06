namespace Hoard.Core.Messages.Quotes;

/// <summary>
/// Refresh quotes for the specified instruments.
/// Raised by <see cref="RefreshQuotesBatchCommand"/>
/// </summary>
/// <param name="InstrumentIds"></param>
/// <param name="CorrelationId">For correlation purposes</param>
public record RefreshQuotesBatchCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds);