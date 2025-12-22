namespace Hoard.Messages.Prices;

public record RefreshPricesBatchBusCommand(
    Guid PricesRunId,
    PipelineMode PipelineMode,
    int InstrumentId, 
    DateOnly StartDate, 
    DateOnly EndDate);