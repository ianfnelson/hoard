namespace Hoard.Messages.Performance;

public record CalculatePositionPerformanceBusCommand(Guid CorrelationId, int InstrumentId, bool IsBackfill);