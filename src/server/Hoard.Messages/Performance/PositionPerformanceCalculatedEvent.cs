namespace Hoard.Messages.Performance;

public record PositionPerformanceCalculatedEvent(Guid CorrelationId, int InstrumentId);