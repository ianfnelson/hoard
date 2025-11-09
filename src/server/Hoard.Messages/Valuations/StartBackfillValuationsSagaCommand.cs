namespace Hoard.Messages.Valuations;

public record StartBackfillValuationsSagaCommand(Guid CorrelationId)
{
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
}
