namespace Hoard.Messages.Positions;

public record CalculatePositionsBusCommand(Guid CorrelationId, bool SuppressCascade = false);