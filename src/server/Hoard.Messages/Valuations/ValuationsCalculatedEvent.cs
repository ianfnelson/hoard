namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);