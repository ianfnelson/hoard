namespace Hoard.Messages.Valuations;

public record CalculateHoldingValuationsBusCommand(Guid ValuationsRunId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);