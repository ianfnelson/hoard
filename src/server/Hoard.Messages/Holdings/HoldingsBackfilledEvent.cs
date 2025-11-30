namespace Hoard.Messages.Holdings;

public record HoldingsBackfilledEvent(Guid CorrelationId, PipelineMode PipelineMode, DateOnly StartDate, DateOnly EndDate);