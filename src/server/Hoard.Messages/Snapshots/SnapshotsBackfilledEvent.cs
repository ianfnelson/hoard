namespace Hoard.Messages.Snapshots;

public record SnapshotsBackfilledEvent(Guid SnapshotsRunId, PipelineMode PipelineMode);
