namespace Hoard.Messages.Quotes;

public record QuoteChangedEvent(Guid CorrelationId, int InstrumentId, DateTime RetrievedUtc);