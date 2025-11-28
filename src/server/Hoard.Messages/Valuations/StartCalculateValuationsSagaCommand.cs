namespace Hoard.Messages.Valuations;

public record StartCalculateValuationsSagaCommand(Guid CorrelationId, PipelineMode PipelineMode, int? InstrumentId, DateOnly? AsOfDate);
