using Hoard.Messages;
using Hoard.Messages.News;
using Rebus.Bus;

namespace Hoard.Core.Application.News;

public record DispatchRefreshNewsCommand(
    Guid NewsRunId,
    PipelineMode PipelineMode,
    IReadOnlyList<int> InstrumentIds)
    : ICommand;

public class DispatchRefreshNewsHandler(IBus bus)
    : ICommandHandler<DispatchRefreshNewsCommand>
{
    public async Task HandleAsync(DispatchRefreshNewsCommand command, CancellationToken ct = default)
    {
        var delay = TimeSpan.Zero;

        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.DeferLocal(delay,
                new RefreshNewsBatchBusCommand(command.NewsRunId, command.PipelineMode, instrumentId));
            delay += TimeSpan.FromSeconds(5);
        }
    }
}
