namespace Hoard.Messages.Holdings;

public record CalculateHoldingsBusCommand(Guid CorrelationId, PipelineMode PipelineMode, DateOnly? AsOfDate);