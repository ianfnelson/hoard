namespace Hoard.Messages.Prices;

public record RefreshPricesBatchBusCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int InstrumentId, 
    DateOnly StartDate, 
    DateOnly EndDate);