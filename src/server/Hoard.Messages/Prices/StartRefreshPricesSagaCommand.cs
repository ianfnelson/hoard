namespace Hoard.Messages.Prices;

public record StartRefreshPricesSagaCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate);
