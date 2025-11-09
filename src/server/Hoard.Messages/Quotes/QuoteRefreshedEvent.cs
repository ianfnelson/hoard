namespace Hoard.Messages.Quotes;

public record QuoteRefreshedEvent(Guid CorrelationId, int InstrumentId, DateTime RetrievedUtc);