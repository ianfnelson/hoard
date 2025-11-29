using Hoard.Messages;
using Hoard.Messages.Performance;
using Rebus.Bus;

namespace Hoard.Core.Application.Performance;

public record DispatchCalculatePortfolioPerformanceCommand(Guid CorrelationId, IReadOnlyList<int> PortfolioIds, PipelineMode PipelineMode)
    : ICommand;

public class DispatchCalculatePortfolioPerformanceHandler(IBus bus)
    : ICommandHandler<DispatchCalculatePortfolioPerformanceCommand>
{
    public async Task HandleAsync(DispatchCalculatePortfolioPerformanceCommand command, CancellationToken ct = default)
    {
        foreach (var portfolioId in command.PortfolioIds)
        {
            await bus.SendLocal(new CalculatePortfolioPerformanceBusCommand(command.CorrelationId, portfolioId, command.PipelineMode));
        }
    }
}