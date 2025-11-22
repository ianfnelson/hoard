namespace Hoard.Messages.Valuations;

public record ValuationChangedEvent(Guid CorrelationId, int InstrumentId, DateOnly AsOfDate);