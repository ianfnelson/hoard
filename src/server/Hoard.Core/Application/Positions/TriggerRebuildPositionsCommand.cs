using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public record TriggerRebuildPositionsCommand(Guid CorrelationId) : ITriggerCommand
{ 
    public object ToBusCommand() => new RebuildPositionsBusCommand(CorrelationId);
}
