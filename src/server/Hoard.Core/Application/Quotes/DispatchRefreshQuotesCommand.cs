using Hoard.Core.Extensions;
using Hoard.Messages.Quotes;
using Rebus.Bus;

namespace Hoard.Core.Application.Quotes;

public record DispatchRefreshQuotesCommand(
    Guid QuotesRunId,
    IReadOnlyList<int> InstrumentIds)
    : ICommand;

public class DispatchRefreshQuotesHandler(IBus bus)
    : ICommandHandler<DispatchRefreshQuotesCommand>
{
    private const int BatchSize = 10;

    public async Task HandleAsync(DispatchRefreshQuotesCommand command, CancellationToken ct = default)
    {
        var shuffled = command.InstrumentIds.Shuffle();

        var delay = TimeSpan.Zero;
        var batchIndex = 0;

        foreach (var batch in shuffled.BatchesOf(BatchSize))
        {
            await bus.DeferLocal(delay,
                new RefreshQuotesBatchBusCommand(command.QuotesRunId, batchIndex, batch));
            delay += TimeSpan.FromSeconds(1);
            batchIndex++;
        }
    }
}
