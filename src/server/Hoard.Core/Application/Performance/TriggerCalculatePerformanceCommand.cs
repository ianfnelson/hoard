using Hoard.Messages.Performance;

namespace Hoard.Core.Application.Performance;

public record TriggerCalculatePerformanceCommand(Guid CorrelationId, int? InstrumentId) : ITriggerCommand
{
    public object ToBusCommand() => new StartCalculatePerformanceSagaCommand(CorrelationId, InstrumentId);
}