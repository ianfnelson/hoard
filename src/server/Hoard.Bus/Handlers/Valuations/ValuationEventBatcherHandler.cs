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
        IHandleMessages<QuoteRefreshedEvent>,
        IHandleMessages<PriceRefreshedEvent>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    public Task Handle(QuoteRefreshedEvent m)
    {
        var date = DateOnly.FromDateTime(m.RetrievedUtc.ToLocalTime());
        buffer.Add(date);
        
        logger.LogDebug("Queued valuation date from Quote: {Date}", date.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(PriceRefreshedEvent m)
    {
        for (var d = m.StartDate; d <= m.EndDate; d = d.AddDays(1))
        {
            buffer.Add(d);
        }
        
        logger.LogDebug("Queued valuation dates from Price range {Start}..{End}", m.StartDate.ToIsoDateString(), m.EndDate.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(HoldingsCalculatedEvent m)
    {
        buffer.Add(m.AsOfDate);
        
        logger.LogDebug("Queued valuation date from Holdings: {Date}", m.AsOfDate.ToIsoDateString());
        return Task.CompletedTask;
    }
}