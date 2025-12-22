namespace Hoard.Messages.Snapshots;

public record SnapshotsCalculatedEvent(Guid SnapshotsRunId, PipelineMode PipelineMode, int Year);