namespace Hoard.Messages.Valuations;

public record PortfolioValuationChangedEvent(Guid CorrelationId, PipelineMode PipelineMode, int PortfolioId, DateOnly AsOfDate);