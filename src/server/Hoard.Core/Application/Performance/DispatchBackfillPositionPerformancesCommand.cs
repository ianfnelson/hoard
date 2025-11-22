using Hoard.Messages.Performances;
using Rebus.Bus;

namespace Hoard.Core.Application.Performance;

public record DispatchBackfillPositionPerformancesCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds)
    : ICommand;

public class DispatchBackfillPositionPerformancesHandler(IBus bus)
    : ICommandHandler<DispatchBackfillPositionPerformancesCommand>
{
    public async Task HandleAsync(DispatchBackfillPositionPerformancesCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(new CalculatePositionPerformancesBusCommand(command.CorrelationId, instrumentId, true));
        }
    }
}