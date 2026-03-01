using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Chrono;

namespace Hoard.Core.Application.Chrono;

public sealed record TriggerCloseOfDayRunCommand(DateOnly? AsOfDate) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CloseOfDay;
    public object ToBusCommand() => new StartCloseOfDaySagaCommand(RunId, AsOfDate, PipelineMode.CloseOfDay);
}