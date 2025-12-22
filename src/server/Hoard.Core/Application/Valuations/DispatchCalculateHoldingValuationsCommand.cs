using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record DispatchCalculateHoldingValuationsCommand(Guid ValuationsRunId, PipelineMode PipelineMode, IReadOnlyList<int> InstrumentIds, DateOnly AsOfDate)
    : ICommand;
    
public class DispatchHoldingsValuationHandler(IBus bus) 
    : ICommandHandler<DispatchCalculateHoldingValuationsCommand>
{
    public async Task HandleAsync(DispatchCalculateHoldingValuationsCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(new CalculateHoldingValuationsBusCommand(command.ValuationsRunId, command.PipelineMode, instrumentId, command.AsOfDate));
        }
    }
}