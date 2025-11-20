namespace Hoard.Messages.Quotes;

public record StockQuoteChangedEvent(Guid CorrelationId, int InstrumentId, DateTime RetrievedUtc);