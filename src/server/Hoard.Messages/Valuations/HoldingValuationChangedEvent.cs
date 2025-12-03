namespace Hoard.Messages.Valuations;

public record HoldingValuationChangedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);