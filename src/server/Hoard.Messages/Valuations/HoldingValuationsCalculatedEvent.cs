namespace Hoard.Messages.Valuations;

public record HoldingValuationsCalculatedEvent(Guid ValuationsRunId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);