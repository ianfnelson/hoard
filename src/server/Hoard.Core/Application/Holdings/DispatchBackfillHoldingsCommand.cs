using Hoard.Messages.Holdings;
using Rebus.Bus;

namespace Hoard.Core.Application.Holdings;

public record DispatchBackfillHoldingsCommand(Guid CorrelationId, IReadOnlyList<DateOnly> Dates)
    : ICommand;

public class DispatchBackfillHoldingsHandler(IBus bus)
    : ICommandHandler<DispatchBackfillHoldingsCommand>
{
    public async Task HandleAsync(DispatchBackfillHoldingsCommand command, CancellationToken ct = default)
    {
        foreach (var date in command.Dates)
        {
            await bus.SendLocal(new CalculateHoldingsBusCommand(command.CorrelationId, date, true));
        }
    }
}