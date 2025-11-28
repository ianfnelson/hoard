using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerBackfillHoldingsCommand(Guid CorrelationId, DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.Backfill)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillHoldingsSagaCommand(CorrelationId, PipelineMode, StartDate, EndDate);
}