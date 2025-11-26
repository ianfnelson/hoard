namespace Hoard.Messages.Performance;

public record StartCalculatePerformanceSagaCommand(Guid CorrelationId, int? InstrumentId);
