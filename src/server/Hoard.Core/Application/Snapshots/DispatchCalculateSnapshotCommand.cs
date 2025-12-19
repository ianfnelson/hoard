using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Rebus.Bus;

namespace Hoard.Core.Application.Snapshots;

public record DispatchCalculateSnapshotCommand(Guid CorrelationId, PipelineMode PipelineMode, IReadOnlyList<int> PortfolioIds, int Year) : ICommand;

public class DispatchCalculateSnapshotHandler(IBus bus)
    : ICommandHandler<DispatchCalculateSnapshotCommand>
{
    public async Task HandleAsync(DispatchCalculateSnapshotCommand command, CancellationToken ct = default)
    {
        foreach (var portfolioId in command.PortfolioIds)
        {
            await bus.SendLocal(new CalculateSnapshotBusCommand(command.CorrelationId, command.PipelineMode,
                portfolioId, command.Year));
        }
    }
}