using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerCalculateHoldingsCommand(Guid CorrelationId, DateOnly? AsOfDate) : 
    ITriggerCommand
{
    public object ToBusCommand() => new CalculateHoldingsBusCommand(CorrelationId, AsOfDate);
}