using Hoard.Messages;
using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public record TriggerCalculatePositionsCommand(Guid CorrelationId, PipelineMode PipelineMode) : ITriggerCommand
{ 
    public object ToBusCommand() => new CalculatePositionsBusCommand(CorrelationId, PipelineMode);
}
