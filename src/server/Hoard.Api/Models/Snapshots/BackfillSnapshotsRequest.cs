using Hoard.Core.Application.Snapshots;

namespace Hoard.Api.Models.Snapshots;

public class BackfillSnapshotsRequest
{
    public int? StartYear { get; init; }
    public int? EndYear { get; init; }
    public int? PortfolioId { get; init; }

    public TriggerBackfillSnapshotsCommand ToCommand()
    {
        return new TriggerBackfillSnapshotsCommand(Guid.NewGuid(), PortfolioId, StartYear, EndYear);
    }
}