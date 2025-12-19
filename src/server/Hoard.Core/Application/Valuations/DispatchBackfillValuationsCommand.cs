using Hoard.Messages;
using Rebus.Bus;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record DispatchBackfillValuationsCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int? InstrumentId,
    IReadOnlyList<DateOnly> Dates) : ICommand;

public class DispatchBackfillValuationsHandler(IBus bus)
    : ICommandHandler<DispatchBackfillValuationsCommand>
{
    public async Task HandleAsync(DispatchBackfillValuationsCommand command, CancellationToken ct = default)
    {
        foreach (var date in command.Dates)
        {
            await bus.SendLocal(new StartCalculateValuationsSagaCommand(command.CorrelationId, command.PipelineMode, command.InstrumentId, date));
        }
    }
}
