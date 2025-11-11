using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerBackfillHoldingsCommand(Guid CorrelationId, DateOnly? StartDate, DateOnly? EndDate)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillHoldingsSagaCommand(CorrelationId, StartDate, EndDate);
}