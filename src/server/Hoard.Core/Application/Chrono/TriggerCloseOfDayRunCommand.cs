using Hoard.Messages;
using Hoard.Messages.Chrono;

namespace Hoard.Core.Application.Chrono;

public record TriggerCloseOfDayRunCommand(Guid CorrelationId, DateOnly? AsOfDate) : ITriggerCommand
{
    public object ToBusCommand() => new StartCloseOfDaySagaCommand(CorrelationId, AsOfDate, PipelineMode.CloseOfDay);
}