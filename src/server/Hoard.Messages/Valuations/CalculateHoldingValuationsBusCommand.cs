namespace Hoard.Messages.Valuations;

public record CalculateHoldingValuationsBusCommand(Guid CorrelationId, PipelineMode PipelineMode, int InstrumentId, DateOnly AsOfDate);