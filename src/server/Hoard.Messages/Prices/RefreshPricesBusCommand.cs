namespace Hoard.Messages.Prices;

public record RefreshPricesBusCommand(Guid CorrelationId, DateOnly? AsOfDate);