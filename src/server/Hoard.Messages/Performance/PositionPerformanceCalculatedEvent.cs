namespace Hoard.Messages.Performance;

public record PositionPerformanceCalculatedEvent(Guid PerformanceRunId, int InstrumentId, PipelineMode PipelineMode);