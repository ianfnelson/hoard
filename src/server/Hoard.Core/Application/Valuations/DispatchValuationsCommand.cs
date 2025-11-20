using Hoard.Messages.Valuations;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record DispatchValuationsCommand(Guid CorrelationId, IReadOnlyList<int> InstrumentIds, DateOnly AsOfDate, bool IsBackfill)
    : ICommand;
    
public class DispatchHoldingsValuationHandler(IBus bus) 
    : ICommandHandler<DispatchValuationsCommand>
{
    public async Task HandleAsync(DispatchValuationsCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(new CalculateValuationsBusCommand(command.CorrelationId, instrumentId, command.AsOfDate, command.IsBackfill));
        }
    }
}