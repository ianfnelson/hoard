namespace Hoard.Messages.Holdings;

public record CalculateHoldingsBusCommand(Guid CorrelationId, DateOnly? AsOfDate);