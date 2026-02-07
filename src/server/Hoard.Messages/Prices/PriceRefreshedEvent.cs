namespace Hoard.Messages.Prices;

public record PriceRefreshedEvent(
    Guid PricesRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    int InstrumentTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime RetrievedUtc);