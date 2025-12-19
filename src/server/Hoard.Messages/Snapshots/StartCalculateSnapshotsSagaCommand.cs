namespace Hoard.Messages.Snapshots;

public record StartCalculateSnapshotsSagaCommand(Guid CorrelationId, PipelineMode PipelineMode, int? PortfolioId, int? Year);