namespace Hoard.Messages.Valuations;

public record StartCalculateValuationsSagaCommand(Guid CorrelationId, DateOnly? AsOfDate);
