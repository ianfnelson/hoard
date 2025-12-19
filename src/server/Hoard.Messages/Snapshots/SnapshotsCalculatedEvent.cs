namespace Hoard.Messages.Snapshots;

public record SnapshotsCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int Year);