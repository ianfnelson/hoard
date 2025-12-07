namespace Hoard.Messages.Valuations;

public record HoldingValuationCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);