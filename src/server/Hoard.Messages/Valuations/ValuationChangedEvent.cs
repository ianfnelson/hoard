namespace Hoard.Messages.Valuations;

public record ValuationChangedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);