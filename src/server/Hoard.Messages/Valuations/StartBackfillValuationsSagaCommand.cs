namespace Hoard.Messages.Valuations;

public record StartBackfillValuationsSagaCommand(Guid CorrelationId, DateOnly? StartDate, DateOnly? EndDate);
