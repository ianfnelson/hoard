namespace Hoard.Messages.Performance;

public record PortfolioPerformancesInvalidatedEvent(Guid CorrelationId, PipelineMode PipelineMode);