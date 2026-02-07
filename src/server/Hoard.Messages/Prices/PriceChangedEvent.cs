namespace Hoard.Messages.Prices;

public record PriceChangedEvent(
    Guid PricesRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    int InstrumentTypeId,
    DateOnly AsOfDate,
    DateTime RetrievedUtc);