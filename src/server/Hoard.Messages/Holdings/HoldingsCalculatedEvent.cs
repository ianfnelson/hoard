namespace Hoard.Messages.Holdings;

public record HoldingsCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, DateOnly AsOfDate);