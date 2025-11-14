using Hoard.Core.Extensions;
using Hoard.Messages.Transactions;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class HoldingEventBatcherHandler(
    IHoldingTriggerBuffer buffer,
    ILogger<HoldingEventBatcherHandler> logger)
    : 
        IHandleMessages<TransactionCreatedEvent>,
        IHandleMessages<TransactionDeletedEvent>,
        IHandleMessages<TransactionUpdatedEvent>
{
    public Task Handle(TransactionCreatedEvent message)
    {
        return AddDatesFrom(message.Date);
    }

    public Task Handle(TransactionDeletedEvent message)
    {
        return AddDatesFrom(message.Date);
    }

    public Task Handle(TransactionUpdatedEvent message)
    {
        return AddDatesFrom(message.Date);
    }

    private Task AddDatesFrom(DateOnly date)
    {
        buffer.AddDatesFrom(date);
        logger.LogDebug("Queued holding dates from {Date}", date.ToIsoDateString());
        return Task.CompletedTask;
    }
}