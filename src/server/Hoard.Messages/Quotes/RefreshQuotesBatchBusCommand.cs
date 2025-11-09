namespace Hoard.Messages.Quotes;

public record RefreshQuotesBatchBusCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds);