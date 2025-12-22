using Hoard.Messages;
using Hoard.Messages.Performance;

namespace Hoard.Core.Application.Performance;

public sealed record TriggerCalculatePerformanceCommand(int? InstrumentId, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid PerformanceRunId { get; } = Guid.NewGuid();
    public object ToBusCommand() => new StartCalculatePerformanceSagaCommand(PerformanceRunId, InstrumentId, PipelineMode);
}