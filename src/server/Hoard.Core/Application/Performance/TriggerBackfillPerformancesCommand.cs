using Hoard.Messages.Performances;

namespace Hoard.Core.Application.Performance;

public record class TriggerBackfillPerformancesCommand(Guid CorrelationId, int? InstrumentId) : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillPerformancesSagaCommand(CorrelationId, InstrumentId);
}