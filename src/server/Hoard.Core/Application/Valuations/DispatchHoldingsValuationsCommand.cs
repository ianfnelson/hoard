using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record DispatchHoldingsValuationsCommand(Guid CorrelationId, PipelineMode PipelineMode, IReadOnlyList<int> InstrumentIds, DateOnly AsOfDate)
    : ICommand;
    
public class DispatchHoldingsValuationHandler(IBus bus) 
    : ICommandHandler<DispatchHoldingsValuationsCommand>
{
    public async Task HandleAsync(DispatchHoldingsValuationsCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(new CalculateHoldingValuationsBusCommand(command.CorrelationId, command.PipelineMode, instrumentId, command.AsOfDate));
        }
    }
}