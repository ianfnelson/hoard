namespace Hoard.Messages.Quotes;

public record RefreshQuotesBatchBusCommand(IReadOnlyList<int> InstrumentIds);