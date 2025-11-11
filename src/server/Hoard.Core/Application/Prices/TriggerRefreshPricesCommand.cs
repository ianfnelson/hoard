using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerRefreshPricesCommand(Guid CorrelationId, DateOnly? AsOfDate) : 
    ITriggerCommand
{
    public object ToBusCommand() => new RefreshPricesBusCommand(CorrelationId, AsOfDate);
}
