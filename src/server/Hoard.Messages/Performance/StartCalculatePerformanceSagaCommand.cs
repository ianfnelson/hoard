namespace Hoard.Messages.Performance;

public record StartCalculatePerformanceSagaCommand(Guid PerformanceRunId, int? InstrumentId, PipelineMode PipelineMode);
