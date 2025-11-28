namespace Hoard.Messages.Performance;

public record PortfolioPerformanceCalculatedEvent(Guid CorrelationId, int PortfolioId, PipelineMode PipelineMode);