namespace Hoard.Messages.Prices;

public record PriceChangedEvent(
    Guid CorrelationId, 
    PipelineMode PipelineMode,
    int InstrumentId,
    bool IsFxPair,
    DateOnly AsOfDate, 
    DateTime RetrievedUtc);