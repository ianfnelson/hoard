namespace Hoard.Messages.Performances;

public record StartBackfillPerformancesSagaCommand(Guid CorrelationId, int? InstrumentId);
