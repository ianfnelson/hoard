namespace Hoard.Messages.Valuations;

public record StartCalculateValuationsSagaCommand(Guid CorrelationId, int? InstrumentId, DateOnly? AsOfDate, bool IsBackfill = false);
