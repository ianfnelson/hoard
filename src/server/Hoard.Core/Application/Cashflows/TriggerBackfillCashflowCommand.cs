using Hoard.Messages.Cashflow;

namespace Hoard.Core.Application.Cashflow;

public record TriggerBackfillCashflowCommand(Guid CorrelationId) : ITriggerCommand
{
    public object ToBusCommand() => new BackfillCashflowBusCommand(CorrelationId);
}