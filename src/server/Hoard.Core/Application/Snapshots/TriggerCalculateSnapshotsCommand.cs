using Hoard.Messages;
using Hoard.Messages.Snapshots;

namespace Hoard.Core.Application.Snapshots;

public sealed record TriggerCalculateSnapshotsCommand(int? Year, PipelineMode PipelineMode = PipelineMode.DaytimeReactive)
    : ITriggerCommand
{
    public Guid SnapshotsRunId { get; } = Guid.NewGuid();
    
    public object ToBusCommand() => new StartCalculateSnapshotsSagaCommand(SnapshotsRunId, PipelineMode, null, Year);
}