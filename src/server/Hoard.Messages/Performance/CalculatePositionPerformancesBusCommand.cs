namespace Hoard.Messages.Performance;

public record CalculatePositionPerformanceBusCommand(Guid PerformanceRunId, int InstrumentId, PipelineMode PipelineMode);