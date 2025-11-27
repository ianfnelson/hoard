using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerBackfillHoldingsCommand(Guid CorrelationId, PipelineMode PipelineMode, DateOnly? StartDate, DateOnly? EndDate)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillHoldingsSagaCommand(CorrelationId, PipelineMode, StartDate, EndDate);
}