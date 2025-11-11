namespace Hoard.Messages.Holdings;

public record StartBackfillHoldingsSagaCommand(Guid CorrelationId, DateOnly? StartDate, DateOnly? EndDate);