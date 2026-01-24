using Hoard.Messages;
using Hoard.Messages.News;

namespace Hoard.Core.Application.News;

public sealed record TriggerRefreshNewsCommand(
    int? InstrumentId,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid NewsRunId { get; } = Guid.NewGuid();

    public object ToBusCommand() => new StartRefreshNewsSagaCommand(
        NewsRunId, PipelineMode, InstrumentId);
}
