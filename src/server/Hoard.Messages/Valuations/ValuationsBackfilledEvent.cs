namespace Hoard.Messages.Valuations;

public record ValuationsBackfilledEvent(Guid CorrelationId, PipelineMode PipelineMode, int? InstrumentId, DateOnly StartDate, DateOnly EndDate);