namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedForDateEvent(Guid CorrelationId, DateOnly AsOfDate, bool IsBackfill);