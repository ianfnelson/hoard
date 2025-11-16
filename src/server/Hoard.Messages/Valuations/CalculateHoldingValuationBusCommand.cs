namespace Hoard.Messages.Valuations;

public record CalculateHoldingValuationBusCommand(Guid CorrelationId, int HoldingId, bool IsBackfill);