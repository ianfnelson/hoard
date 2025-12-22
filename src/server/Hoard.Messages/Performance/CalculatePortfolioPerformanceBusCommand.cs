namespace Hoard.Messages.Performance;

public record CalculatePortfolioPerformanceBusCommand(Guid PerformanceRunId, int PortfolioId, PipelineMode PipelineMode);