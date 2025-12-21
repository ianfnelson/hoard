using Hoard.Messages;
using Hoard.Messages.Positions;

namespace Hoard.Core.Application.Positions;

public sealed record TriggerCalculatePositionsCommand : ITriggerCommand
{ 
    public Guid PositionsRunId { get; }
    public PipelineMode PipelineMode { get; }

    private TriggerCalculatePositionsCommand(Guid positionsRunId, PipelineMode pipelineMode)
    {
        PositionsRunId = positionsRunId;
        PipelineMode = pipelineMode;
    }

    public static TriggerCalculatePositionsCommand Create(
        PipelineMode pipelineMode = PipelineMode.DaytimeReactive)
    {
        return new TriggerCalculatePositionsCommand(Guid.NewGuid(), pipelineMode);
    }
    
    public object ToBusCommand() => new CalculatePositionsBusCommand(PositionsRunId, PipelineMode);
}
