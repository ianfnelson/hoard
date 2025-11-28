namespace Hoard.Messages.Performance;

public record CalculatePortfolioPerformanceBusCommand(Guid CorrelationId, int PortfolioId, PipelineMode PipelineMode);