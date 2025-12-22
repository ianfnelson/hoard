using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Transactions;

public class TransactionsEventHandler(IMediator mediator) :
    IHandleMessages<TransactionCreatedEvent>,
    IHandleMessages<TransactionDeletedEvent>,
    IHandleMessages<TransactionUpdatedEvent>
{
    public Task Handle(TransactionCreatedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.PipelineMode, message.Date);
    }

    public Task Handle(TransactionDeletedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.PipelineMode, message.Date);
    }

    public Task Handle(TransactionUpdatedEvent message)
    {
        return CalculateHoldingsForDatesFrom(message.PipelineMode, message.Date);
    }
    
    private async Task CalculateHoldingsForDatesFrom(PipelineMode pipelineMode, DateOnly date)
    {
        var today = DateOnlyHelper.TodayLocal();

        for (var d = date; d <= today; d = d.AddDays(1))
        {
            var command = new ProcessCalculateHoldingsCommand(Guid.NewGuid(), pipelineMode, d);
            await mediator.SendAsync(command);
        }
    }
}