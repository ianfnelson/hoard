namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedForDateEvent(Guid CorrelationId, PipelineMode PipelineMode, DateOnly AsOfDate);