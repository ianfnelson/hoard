namespace Hoard.Messages.Snapshots;

public record CalculateSnapshotBusCommand(Guid SnapshotsRunId, PipelineMode PipelineMode, int PortfolioId, int Year);