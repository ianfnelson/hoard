using Hoard.Messages.Performance;
using Rebus.Bus;

namespace Hoard.Core.Application.Performance;

public record DispatchCalculatePositionPerformanceCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds)
    : ICommand;

public class DispatchCalculatePositionPerformanceHandler(IBus bus)
    : ICommandHandler<DispatchCalculatePositionPerformanceCommand>
{
    public async Task HandleAsync(DispatchCalculatePositionPerformanceCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(new CalculatePositionPerformanceBusCommand(command.CorrelationId, instrumentId, true));
        }
    }
}