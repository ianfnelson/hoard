namespace Hoard.Messages.Prices;

public record StartRefreshPricesSagaCommand(
    Guid PricesRunId,
    PipelineMode PipelineMode,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate);
