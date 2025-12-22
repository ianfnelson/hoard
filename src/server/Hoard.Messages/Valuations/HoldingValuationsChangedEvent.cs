namespace Hoard.Messages.Valuations;

public record HoldingValuationsChangedEvent(Guid ValuationsRunId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);