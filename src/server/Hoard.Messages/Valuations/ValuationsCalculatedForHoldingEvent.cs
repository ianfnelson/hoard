namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedForHoldingEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);