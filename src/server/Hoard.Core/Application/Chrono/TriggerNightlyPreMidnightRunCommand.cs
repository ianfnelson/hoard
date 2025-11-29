using Hoard.Messages;
using Hoard.Messages.Chrono;

namespace Hoard.Core.Application.Chrono;

public record TriggerNightlyPreMidnightRunCommand(Guid CorrelationId, DateOnly? AsOfDate) : ITriggerCommand
{
    public object ToBusCommand() => new StartNightlySagaCommand(CorrelationId, AsOfDate, PipelineMode.NightlyPreMidnight);
}