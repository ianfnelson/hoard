namespace Hoard.Messages.Positions;

public record CalculatePositionsBusCommand(Guid CorrelationId, PipelineMode PipelineMode);