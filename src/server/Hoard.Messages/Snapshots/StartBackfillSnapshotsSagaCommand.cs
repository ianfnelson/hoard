namespace Hoard.Messages.Snapshots;

public record StartBackfillSnapshotsSagaCommand(Guid SnapshotsRunId, PipelineMode PipelineMode, int? PortfolioId, int? StartYear, int? EndYear);