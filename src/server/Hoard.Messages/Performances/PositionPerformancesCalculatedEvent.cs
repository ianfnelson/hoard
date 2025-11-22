namespace Hoard.Messages.Performances;

public record PositionPerformancesCalculatedEvent(Guid CorrelationId, int InstrumentId);