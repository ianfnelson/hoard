using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public sealed record TriggerCalculatePositionsCommand(PipelineMode PipelineMode = PipelineMode.DaytimeReactive)
    : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CalculatePositions;

    public object ToBusCommand() => new CalculatePositionsBusCommand(RunId, PipelineMode);
}
