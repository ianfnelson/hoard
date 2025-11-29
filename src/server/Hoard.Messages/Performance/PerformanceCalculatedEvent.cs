namespace Hoard.Messages.Performance;

public record PerformanceCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode);