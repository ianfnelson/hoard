namespace Hoard.Messages.Quotes;

public record QuoteChangedEvent(
    Guid CorrelationId, 
    PipelineMode PipelineMode, 
    int InstrumentId, 
    bool IsFxPair,
    DateTime RetrievedUtc);