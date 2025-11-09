namespace Hoard.Messages.Prices;

public record StartBackfillPricesSagaCommand(Guid CorrelationId)
{
    public int? InstrumentId { get; init; }
    
    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }
}
