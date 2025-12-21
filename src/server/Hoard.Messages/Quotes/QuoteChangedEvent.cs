namespace Hoard.Messages.Quotes;

public record QuoteChangedEvent(int InstrumentId, bool IsFxPair, DateTime RetrievedUtc);