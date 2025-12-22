namespace Hoard.Messages.Valuations;

public record ValuationsBackfilledEvent(Guid ValuationsRunId, PipelineMode PipelineMode, int? InstrumentId, DateOnly StartDate, DateOnly EndDate);