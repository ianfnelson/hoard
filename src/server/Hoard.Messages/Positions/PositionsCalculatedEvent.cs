namespace Hoard.Messages.Positions;

public record PositionsCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode);