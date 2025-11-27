namespace Hoard.Messages.Prices;

public record PricesRefreshedEvent(
    Guid CorrelationId,
    PipelineMode PipelineMode);