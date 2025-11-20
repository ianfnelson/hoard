using Hoard.Core.Application;
using Hoard.Core.Application.Cashflows;
using Hoard.Messages.Cashflow;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Cashflows;

public class BackfillCashflowBusHandler(IMediator mediator)
: IHandleMessages<BackfillCashflowBusCommand>
{
    public async Task Handle(BackfillCashflowBusCommand message)
    {
        var appCommand = new ProcessBackfillCashflowCommand(message.CorrelationId);
        await mediator.SendAsync(appCommand);
    }
}