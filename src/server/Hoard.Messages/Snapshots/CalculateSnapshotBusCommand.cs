namespace Hoard.Messages.Snapshots;

public record CalculateSnapshotBusCommand(Guid CorrelationId, PipelineMode PipelineMode, int PortfolioId, int Year);