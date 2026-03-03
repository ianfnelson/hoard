namespace Hoard.Messages.Quotes;

public record RefreshQuotesBatchBusCommand(Guid QuotesRunId, int BatchIndex, IReadOnlyList<int> InstrumentIds);