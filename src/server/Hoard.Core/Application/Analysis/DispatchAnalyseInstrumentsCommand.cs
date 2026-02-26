using Hoard.Messages;
using Hoard.Messages.Analysis;
using Rebus.Bus;

namespace Hoard.Core.Application.Analysis;

public record DispatchAnalyseInstrumentsCommand(
    Guid AnalysisRunId,
    PipelineMode PipelineMode,
    IReadOnlyList<int> InstrumentIds,
    DateOnly AsOfDate) : ICommand;

public class DispatchAnalyseInstrumentsHandler(IBus bus)
    : ICommandHandler<DispatchAnalyseInstrumentsCommand>
{
    public async Task HandleAsync(DispatchAnalyseInstrumentsCommand command, CancellationToken ct = default)
    {
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.SendLocal(
                new AnalyseInstrumentBatchBusCommand(command.AnalysisRunId, command.PipelineMode, instrumentId, command.AsOfDate));
        }
    }
}
