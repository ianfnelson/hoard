namespace Hoard.Messages.Prices;

public record RefreshPricesBatchBusCommand(
    Guid CorrelationId, 
    int InstrumentId, 
    DateOnly StartDate, 
    DateOnly EndDate, 
    bool IsBackfill = false);