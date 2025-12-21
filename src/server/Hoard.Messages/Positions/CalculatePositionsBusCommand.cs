namespace Hoard.Messages.Positions;

public record CalculatePositionsBusCommand(Guid PositionsRunId, PipelineMode PipelineMode);