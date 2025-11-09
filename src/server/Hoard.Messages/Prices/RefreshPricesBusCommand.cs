namespace Hoard.Messages.Prices;

public record RefreshPricesBusCommand(Guid CorrelationId)
{
    public DateOnly? AsOfDate { get; init; }
}