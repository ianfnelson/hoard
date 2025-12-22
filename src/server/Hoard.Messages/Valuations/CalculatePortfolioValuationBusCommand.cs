namespace Hoard.Messages.Valuations;

public record CalculatePortfolioValuationBusCommand(
    Guid ValuationsRunId,
    PipelineMode PipelineMode,
    int PortfolioId,
    DateOnly AsOfDate);
