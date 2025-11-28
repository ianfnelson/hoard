using Hoard.Messages;
using Hoard.Messages.Performance;

namespace Hoard.Core.Application.Performance;

public record TriggerCalculatePerformanceCommand(Guid CorrelationId, int? InstrumentId, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public object ToBusCommand() => new StartCalculatePerformanceSagaCommand(CorrelationId, InstrumentId, PipelineMode);
}