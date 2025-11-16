namespace Hoard.Messages.Valuations;

public record HoldingValuationCalculatedEvent(Guid CorrelationId, int HoldingId, DateOnly AsOfDate, bool IsBackfill);