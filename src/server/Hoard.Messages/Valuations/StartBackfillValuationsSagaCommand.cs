namespace Hoard.Messages.Valuations;

public record StartBackfillValuationsSagaCommand(Guid ValuationsRunId, PipelineMode PipelineMode, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate);
