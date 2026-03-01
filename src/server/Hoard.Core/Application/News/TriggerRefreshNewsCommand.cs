using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.News;

namespace Hoard.Core.Application.News;

public sealed record TriggerRefreshNewsCommand(
    int? InstrumentId,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.RefreshNews;

    public object ToBusCommand() => new StartRefreshNewsSagaCommand(
        RunId, PipelineMode, InstrumentId);
}
