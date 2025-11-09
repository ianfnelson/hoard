namespace Hoard.Messages.Holdings;

public record CalculateHoldingsBusCommand(Guid CorrelationId)
{
    public DateOnly? AsOfDate { get; init; }
}