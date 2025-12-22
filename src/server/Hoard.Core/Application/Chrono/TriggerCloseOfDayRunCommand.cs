using Hoard.Messages;
using Hoard.Messages.Chrono;

namespace Hoard.Core.Application.Chrono;

public sealed record TriggerCloseOfDayRunCommand(DateOnly? AsOfDate) : ITriggerCommand
{
    public Guid ChronoRunId { get; } = Guid.NewGuid();
    public object ToBusCommand() => new StartCloseOfDaySagaCommand(ChronoRunId, AsOfDate, PipelineMode.CloseOfDay);
}