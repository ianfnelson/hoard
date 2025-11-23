namespace Hoard.Messages.Valuations;

public record StartBackfillValuationsSagaCommand(Guid CorrelationId, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate);
