using Hoard.Messages;
using Hoard.Messages.Snapshots;

namespace Hoard.Core.Application.Snapshots;

public record TriggerBackfillSnapshotsCommand(
    Guid CorrelationId,
    int? PortfolioId,
    int? StartYear,
    int? EndYear,
    PipelineMode PipelineMode = PipelineMode.Backfill)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillSnapshotsSagaCommand(CorrelationId, PipelineMode, PortfolioId, StartYear, EndYear);
}
