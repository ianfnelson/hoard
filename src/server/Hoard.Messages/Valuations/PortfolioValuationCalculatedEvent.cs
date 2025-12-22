namespace Hoard.Messages.Valuations;

public record PortfolioValuationCalculatedEvent(Guid ValuationsRunId, PipelineMode PipelineMode, int PortfolioId, DateOnly AsOfDate);