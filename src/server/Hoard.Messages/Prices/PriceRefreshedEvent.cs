namespace Hoard.Messages.Prices;

public record PriceRefreshedEvent(
    Guid CorrelationId, 
    PipelineMode PipelineMode,
    int InstrumentId, 
    bool IsFxPair,
    DateOnly StartDate, 
    DateOnly EndDate, 
    DateTime RetrievedUtc);