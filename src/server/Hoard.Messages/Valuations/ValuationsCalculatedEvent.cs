namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedEvent(Guid CorrelationId, int InstrumentId, DateOnly AsOfDate, bool IsBackfill);