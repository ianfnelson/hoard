namespace Hoard.Messages.Valuations;

public record StartBackfillValuationsSagaCommand(Guid CorrelationId, PipelineMode PipelineMode, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate);
