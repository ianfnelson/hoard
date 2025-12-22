namespace Hoard.Messages.Snapshots;

public record StartCalculateSnapshotsSagaCommand(Guid SnapshotsRunId, PipelineMode PipelineMode, int? PortfolioId, int? Year);