using Hoard.Messages;
using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public sealed record TriggerCalculatePositionsCommand(PipelineMode PipelineMode = PipelineMode.DaytimeReactive)
    : ITriggerCommand
{
    public Guid PositionsRunId { get; } = Guid.NewGuid();

    public object ToBusCommand() => new CalculatePositionsBusCommand(PositionsRunId, PipelineMode);
}
