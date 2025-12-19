using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Rebus.Bus;

namespace Hoard.Core.Application.Snapshots;

public record DispatchBackfillSnapshotsCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int? PortfolioId,
    IReadOnlyList<int> Years)
    : ICommand;

public class DispatchBackfillSnapshotsHandler(IBus bus)
    : ICommandHandler<DispatchBackfillSnapshotsCommand>
{
    public async Task HandleAsync(DispatchBackfillSnapshotsCommand command, CancellationToken ct = default)
    {
        foreach (var year in command.Years)
        {
            await bus.SendLocal(new StartCalculateSnapshotsSagaCommand(command.CorrelationId, command.PipelineMode,
                command.PortfolioId, year));
        }
    }
}