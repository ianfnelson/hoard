namespace Hoard.Messages.Valuations;

public record HoldingValuationsCalculatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);