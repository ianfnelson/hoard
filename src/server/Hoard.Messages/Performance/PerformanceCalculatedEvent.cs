namespace Hoard.Messages.Performance;

public record PerformanceCalculatedEvent(Guid PerformanceRunId, PipelineMode PipelineMode);