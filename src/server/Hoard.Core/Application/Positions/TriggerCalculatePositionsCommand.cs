using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public record TriggerCalculatePositionsCommand(Guid CorrelationId) : ITriggerCommand
{ 
    public object ToBusCommand() => new CalculatePositionsBusCommand(CorrelationId);
}
