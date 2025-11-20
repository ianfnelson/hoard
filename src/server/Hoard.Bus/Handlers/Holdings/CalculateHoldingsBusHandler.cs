using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages.Holdings;
using Hoard.Messages.Transactions;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class CalculateHoldingsBusHandler(IMediator mediator) 
    : IHandleMessages<CalculateHoldingsBusCommand>,
        IHandleMessages<TransactionCreatedEvent>,
        IHandleMessages<TransactionDeletedEvent>,
        IHandleMessages<TransactionUpdatedEvent>
{
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsCommand(message.CorrelationId, message.AsOfDate, message.IsBackfill);
        await mediator.SendAsync(appCommand);
    }

    public Task Handle(TransactionCreatedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.CorrelationId, message.Date);
    }

    public Task Handle(TransactionDeletedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.CorrelationId, message.Date);
    }

    public Task Handle(TransactionUpdatedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.CorrelationId, message.Date);
    }
    
    private async Task CalculateHoldingsForDatesFrom(Guid correlationId, DateOnly date)
    {
        var today = DateOnlyHelper.TodayLocal();

        for (var d = date; d <= today; d = d.AddDays(1))
        {
            var command = new ProcessCalculateHoldingsCommand(correlationId, d);
            await mediator.SendAsync(command);
        }
    }
}