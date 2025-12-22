using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record DispatchCalculatePortfolioValuationCommand(Guid ValuationsRunId, PipelineMode PipelineMode, IReadOnlyList<int> PortfolioIds, DateOnly AsOfDate)
    : ICommand;
    
public class DispatchCalculatePortfolioValuationHandler(IBus bus) 
    : ICommandHandler<DispatchCalculatePortfolioValuationCommand>
{
    public async Task HandleAsync(DispatchCalculatePortfolioValuationCommand command, CancellationToken ct = default)
    {
        foreach (var portfolioId in command.PortfolioIds)
        {
            await bus.SendLocal(new CalculatePortfolioValuationBusCommand(command.ValuationsRunId, command.PipelineMode, portfolioId, command.AsOfDate));
        }
    }
}