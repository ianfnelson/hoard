using Hoard.Messages;
using Hoard.Messages.Performance;
using Rebus.Bus;

namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePortfolioPerformanceCommand(Guid CorrelationId, int PortfolioId, PipelineMode PipelineMode) : ICommand;

public class ProcessCalculatePortfolioPerformanceHandler(
    IBus bus) : ICommandHandler<ProcessCalculatePortfolioPerformanceCommand>
{
    public async Task HandleAsync(ProcessCalculatePortfolioPerformanceCommand command, CancellationToken ct = default)
    {
        var (correlationId, portfolioId, pipelineMode) = command;
        await bus.Publish(new PortfolioPerformanceCalculatedEvent(correlationId, portfolioId, pipelineMode));
    }
}