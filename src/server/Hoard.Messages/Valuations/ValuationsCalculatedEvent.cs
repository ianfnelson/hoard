namespace Hoard.Messages.Valuations;

public record ValuationsCalculatedEvent(Guid CorrelationId, DateOnly AsOfDate);