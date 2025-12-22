namespace Hoard.Messages.Holdings;

public record HoldingsBackfilledEvent(Guid HoldingsRunId, PipelineMode PipelineMode, DateOnly StartDate, DateOnly EndDate);