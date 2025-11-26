using Hoard.Messages.Performance;

namespace Hoard.Core.Application.Performance;

public record class TriggerCalculatePerformanceCommand(Guid CorrelationId, int? InstrumentId) : ITriggerCommand
{
    public object ToBusCommand() => new StartCalculatePerformanceSagaCommand(CorrelationId, InstrumentId);
}