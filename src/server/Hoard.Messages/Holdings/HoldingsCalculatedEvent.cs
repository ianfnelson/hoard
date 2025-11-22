namespace Hoard.Messages.Holdings;

public record HoldingsCalculatedEvent(Guid CorrelationId, DateOnly AsOfDate, bool IsBackfill);