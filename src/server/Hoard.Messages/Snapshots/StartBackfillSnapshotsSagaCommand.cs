namespace Hoard.Messages.Snapshots;

public record StartBackfillSnapshotsSagaCommand(Guid CorrelationId, PipelineMode PipelineMode, int? PortfolioId, int? StartYear, int? EndYear);