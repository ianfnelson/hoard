using Hoard.Core.Extensions;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class ValuationEventBatcherHandler(
    IValuationTriggerBuffer buffer,
    ILogger<ValuationEventBatcherHandler> logger)
    :
        IHandleMessages<QuoteChangedEvent>,
        IHandleMessages<PriceChangedEvent>,
        IHandleMessages<HoldingChangedEvent>
{
    public Task Handle(QuoteChangedEvent m)
    {
        var date = DateOnly.FromDateTime(m.RetrievedUtc.ToLocalTime());
        buffer.Add(date);
        
        logger.LogDebug("Queued valuation date from Quote: {Date}", date.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(PriceChangedEvent m)
    {
        buffer.Add(m.AsOfDate);
        
        logger.LogDebug("Queued valuation date from Price: {Date}", m.AsOfDate.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(HoldingChangedEvent m)
    {
        buffer.Add(m.AsOfDate);
        
        logger.LogDebug("Queued valuation date from Holdings: {Date}", m.AsOfDate.ToIsoDateString());
        return Task.CompletedTask;
    }
}