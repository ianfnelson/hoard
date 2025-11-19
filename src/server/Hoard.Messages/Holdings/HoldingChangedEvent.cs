namespace Hoard.Messages.Holdings;

public record HoldingChangedEvent(Guid CorrelationId, DateOnly AsOfDate, int InstrumentId);