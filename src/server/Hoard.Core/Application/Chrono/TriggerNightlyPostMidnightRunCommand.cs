using Hoard.Messages;
using Hoard.Messages.Chrono;

namespace Hoard.Core.Application.Chrono;

public record TriggerNightlyPostMidnightRunCommand(Guid CorrelationId, DateOnly? AsOfDate) : ITriggerCommand
{
    public object ToBusCommand() => new StartNightlySagaCommand(CorrelationId, AsOfDate, PipelineMode.NightlyPostMidnight);
}