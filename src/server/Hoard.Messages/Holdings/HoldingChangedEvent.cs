namespace Hoard.Messages.Holdings;

public record HoldingChangedEvent(Guid CorrelationId, PipelineMode PipelineMode, DateOnly AsOfDate, int InstrumentId);