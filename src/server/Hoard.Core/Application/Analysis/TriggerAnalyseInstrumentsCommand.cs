using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Analysis;

namespace Hoard.Core.Application.Analysis;

public sealed record TriggerAnalyseInstrumentsCommand(
    DateOnly AsOfDate,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.AnalyseInstruments;

    public object ToBusCommand() => new StartAnalyseInstrumentsSagaCommand(
        RunId, PipelineMode, AsOfDate);
}
