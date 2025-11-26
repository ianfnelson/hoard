namespace Hoard.Messages.Positions;

public record PositionsCalculatedEvent(Guid CorrelationId, bool SuppressCascade);