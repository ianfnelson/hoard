namespace Hoard.Messages.Positions;

public record PositionsCalculatedEvent(Guid PositionsRunId, PipelineMode PipelineMode);