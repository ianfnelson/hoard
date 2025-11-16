namespace Hoard.Messages.Prices;

public record PriceRefreshedEvent(
    Guid CorrelationId, 
    int InstrumentId, 
    DateOnly StartDate, 
    DateOnly EndDate, 
    DateTime RetrievedUtc, 
    bool IsBackfill);