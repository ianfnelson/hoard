namespace Hoard.Messages.Valuations;

public record PortfolioValuationChangedEvent(Guid ValuationsRunId, PipelineMode PipelineMode, int PortfolioId, DateOnly AsOfDate);