namespace Hoard.Messages.Valuations;

public record CalculateValuationsBusCommand(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);