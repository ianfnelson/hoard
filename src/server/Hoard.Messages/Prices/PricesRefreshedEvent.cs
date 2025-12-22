namespace Hoard.Messages.Prices;

public record PricesRefreshedEvent(
    Guid PricesRunId,
    PipelineMode PipelineMode);