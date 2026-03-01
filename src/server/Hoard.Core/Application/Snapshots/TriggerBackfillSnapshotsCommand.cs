using Hoard.Core.Domain.Entities;
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
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.BackfillSnapshots;
    public object ToBusCommand() => new StartBackfillSnapshotsSagaCommand(RunId, PipelineMode, PortfolioId, StartYear, EndYear);
}
