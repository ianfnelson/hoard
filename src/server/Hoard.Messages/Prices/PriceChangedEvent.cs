namespace Hoard.Messages.Prices;

public record PriceChangedEvent(
    Guid PricesRunId, 
    PipelineMode PipelineMode,
    int InstrumentId,
    bool IsFxPair,
    DateOnly AsOfDate, 
    DateTime RetrievedUtc);