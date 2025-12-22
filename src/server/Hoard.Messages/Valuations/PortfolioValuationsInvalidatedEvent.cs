namespace Hoard.Messages.Valuations;

public record PortfolioValuationsInvalidatedEvent(PipelineMode PipelineMode, DateOnly AsOfDate);