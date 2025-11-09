namespace Hoard.Messages.Valuations;

public record StartCalculateValuationsSagaCommand(Guid CorrelationId)
{
    public DateOnly? AsOfDate { get; init; }
}