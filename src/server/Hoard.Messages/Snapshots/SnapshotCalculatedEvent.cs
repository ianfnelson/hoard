namespace Hoard.Messages.Snapshots;

public record SnapshotCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int Year, int PortfolioId);