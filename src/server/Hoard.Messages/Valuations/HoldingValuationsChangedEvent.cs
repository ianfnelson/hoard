namespace Hoard.Messages.Valuations;

public record HoldingValuationsChangedEvent(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);