namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedEvent(Guid ValuationsRunId, PipelineMode PipelineMode, DateOnly AsOfDate);