using Hoard.Messages;
using Hoard.Messages.Snapshots;

namespace Hoard.Core.Application.Snapshots;

public record TriggerBackfillSnapshotsCommand(
    int? PortfolioId,
    int? StartYear,
    int? EndYear,
    PipelineMode PipelineMode = PipelineMode.Backfill)
    : ITriggerCommand
{
    public Guid SnapshotsRunId { get; } = Guid.NewGuid();
    public object ToBusCommand() => new StartBackfillSnapshotsSagaCommand(SnapshotsRunId, PipelineMode, PortfolioId, StartYear, EndYear);
}
