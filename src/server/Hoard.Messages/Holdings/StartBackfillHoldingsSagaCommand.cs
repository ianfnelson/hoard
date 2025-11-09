namespace Hoard.Messages.Holdings;

public record StartBackfillHoldingsSagaCommand(Guid CorrelationId)
{
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
}