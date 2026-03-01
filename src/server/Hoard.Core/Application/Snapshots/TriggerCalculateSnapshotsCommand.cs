using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Snapshots;

namespace Hoard.Core.Application.Snapshots;

public sealed record TriggerCalculateSnapshotsCommand(int? Year, PipelineMode PipelineMode = PipelineMode.DaytimeReactive)
    : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CalculateSnapshots;
    
    public object ToBusCommand() => new StartCalculateSnapshotsSagaCommand(RunId, PipelineMode, null, Year);
}