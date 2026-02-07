namespace Hoard.Messages.Quotes;

public record QuoteChangedEvent(int InstrumentId, int InstrumentTypeId, DateTime RetrievedUtc);