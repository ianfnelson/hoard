namespace Hoard.Messages.Valuations;

public record CalculateValuationsBusCommand(Guid CorrelationId, int InstrumentId, DateOnly AsOfDate, bool IsBackfill);