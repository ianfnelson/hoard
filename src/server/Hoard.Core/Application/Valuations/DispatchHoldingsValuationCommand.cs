using Hoard.Messages.Valuations;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record DispatchHoldingsValuationCommand(Guid CorrelationId, IReadOnlyList<int> HoldingIds, bool IsBackfill)
    : ICommand;
    
public class DispatchHoldingsValuationHandler(IBus bus) 
    : ICommandHandler<DispatchHoldingsValuationCommand>
{
    public async Task HandleAsync(DispatchHoldingsValuationCommand command, CancellationToken ct = default)
    {
        foreach (var holdingId in command.HoldingIds)
        {
            await bus.SendLocal(new CalculateHoldingValuationBusCommand(command.CorrelationId, holdingId, command.IsBackfill));
        }
    }
}