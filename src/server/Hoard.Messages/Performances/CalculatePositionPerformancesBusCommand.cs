namespace Hoard.Messages.Performances;

public record CalculatePositionPerformancesBusCommand(Guid CorrelationId, int InstrumentId, bool IsBackfill);