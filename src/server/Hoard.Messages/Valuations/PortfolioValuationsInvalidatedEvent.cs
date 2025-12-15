namespace Hoard.Messages.Valuations;

public record PortfolioValuationsInvalidatedEvent(Guid CorrelationId, PipelineMode PipelineMode, DateOnly AsOfDate);