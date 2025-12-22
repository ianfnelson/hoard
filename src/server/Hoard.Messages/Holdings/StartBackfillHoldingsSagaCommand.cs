namespace Hoard.Messages.Holdings;

public record StartBackfillHoldingsSagaCommand(Guid HoldingsRunId, PipelineMode PipelineMode, DateOnly? StartDate, DateOnly? EndDate);