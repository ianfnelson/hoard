using Hoard.Messages;
using Hoard.Messages.Analysis;

namespace Hoard.Core.Application.Analysis;

public sealed record TriggerAnalyseInstrumentsCommand(
    DateOnly AsOfDate,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid AnalysisRunId { get; } = Guid.NewGuid();

    public object ToBusCommand() => new StartAnalyseInstrumentsSagaCommand(
        AnalysisRunId, PipelineMode, AsOfDate);
}
