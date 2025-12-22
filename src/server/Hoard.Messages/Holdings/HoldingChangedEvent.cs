namespace Hoard.Messages.Holdings;

public record HoldingChangedEvent(Guid HoldingsRunId, PipelineMode PipelineMode, DateOnly AsOfDate, int InstrumentId);