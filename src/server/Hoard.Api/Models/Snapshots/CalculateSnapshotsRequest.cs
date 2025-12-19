using Hoard.Core.Application.Snapshots;

namespace Hoard.Api.Models.Snapshots;

public class CalculateSnapshotsRequest
{
    public int? Year { get; init; }

    public TriggerCalculateSnapshotsCommand ToCommand()
    {
        return new TriggerCalculateSnapshotsCommand(Guid.NewGuid(), Year);
    }
}