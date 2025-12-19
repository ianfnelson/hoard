using Hoard.Messages;
using Hoard.Messages.Snapshots;

namespace Hoard.Core.Application.Snapshots;

public record TriggerCalculateSnapshotsCommand(Guid CorrelationId, int? Year, PipelineMode PipelineMode = PipelineMode.DaytimeReactive)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartCalculateSnapshotsSagaCommand(CorrelationId, PipelineMode, null, Year);
}