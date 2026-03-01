using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Performance;

namespace Hoard.Core.Application.Performance;

public sealed record TriggerCalculatePerformanceCommand(int? InstrumentId, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CalculatePerformance;
    public object ToBusCommand() => new StartCalculatePerformanceSagaCommand(RunId, InstrumentId, PipelineMode);
}