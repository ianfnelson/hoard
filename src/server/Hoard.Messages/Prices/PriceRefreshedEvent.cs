namespace Hoard.Messages.Prices;

public record PriceRefreshedEvent(
    Guid PricesRunId, 
    PipelineMode PipelineMode,
    int InstrumentId, 
    bool IsFxPair,
    DateOnly StartDate, 
    DateOnly EndDate, 
    DateTime RetrievedUtc);