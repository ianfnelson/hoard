namespace Hoard.Messages.Valuations;

public record StartCalculateValuationsSagaCommand(Guid ValuationsRunId, PipelineMode PipelineMode, int? InstrumentId, DateOnly? AsOfDate);
