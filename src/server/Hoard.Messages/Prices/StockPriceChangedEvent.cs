namespace Hoard.Messages.Prices;

public record StockPriceChangedEvent(
    Guid CorrelationId, 
    int InstrumentId, 
    DateOnly AsOfDate, 
    DateTime RetrievedUtc);