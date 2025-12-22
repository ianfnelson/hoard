namespace Hoard.Messages.Performance;

public record PortfolioPerformanceCalculatedEvent(Guid PerformanceRunId, int PortfolioId, PipelineMode PipelineMode);