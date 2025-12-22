namespace Hoard.Messages.Snapshots;

public record SnapshotCalculatedEvent(Guid SnapshotsRunId, PipelineMode PipelineMode, int Year, int PortfolioId);