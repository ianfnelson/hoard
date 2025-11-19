namespace Hoard.Messages.Prices;

public record PriceChangedEvent(
    Guid CorrelationId, 
    int InstrumentId, 
    DateOnly AsOfDate, 
    DateTime RetrievedUtc);