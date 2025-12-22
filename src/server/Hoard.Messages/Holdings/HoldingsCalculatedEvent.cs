namespace Hoard.Messages.Holdings;

public record HoldingsCalculatedEvent(Guid HoldingsRunId, PipelineMode PipelineMode, DateOnly AsOfDate);