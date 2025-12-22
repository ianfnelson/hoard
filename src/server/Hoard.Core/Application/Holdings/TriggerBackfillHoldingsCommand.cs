using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public sealed record TriggerBackfillHoldingsCommand(DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.Backfill)
    : ITriggerCommand
{
    public Guid HoldingsRunId { get; } = Guid.NewGuid();
    public object ToBusCommand() => new StartBackfillHoldingsSagaCommand(HoldingsRunId, PipelineMode, StartDate, EndDate);
}