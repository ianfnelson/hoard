namespace Hoard.Messages.Valuations;

public record PortfolioValuationCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int PortfolioId, DateOnly AsOfDate);