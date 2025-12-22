namespace Hoard.Messages.Holdings;

public record CalculateHoldingsBusCommand(Guid HoldingsRunId, PipelineMode PipelineMode, DateOnly? AsOfDate);