namespace Hoard.Messages.Holdings;

public record StartBackfillHoldingsSagaCommand(Guid CorrelationId, PipelineMode PipelineMode, DateOnly? StartDate, DateOnly? EndDate);