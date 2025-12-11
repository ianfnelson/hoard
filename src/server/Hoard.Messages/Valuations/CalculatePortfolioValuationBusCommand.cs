namespace Hoard.Messages.Valuations;

public record CalculatePortfolioValuationBusCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int PortfolioId,
    DateOnly AsOfDate);
