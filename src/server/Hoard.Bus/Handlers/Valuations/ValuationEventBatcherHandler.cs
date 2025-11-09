using Hoard.Core.Extensions;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class ValuationEventBatcherHandler :
    IHandleMessages<QuoteRefreshedEvent>,
    IHandleMessages<PriceRefreshedEvent>,
    IHandleMessages<HoldingsCalculatedEvent>
{
    private readonly IValuationTriggerBuffer _buffer;
    private readonly ILogger<ValuationEventBatcherHandler> _logger;

    public ValuationEventBatcherHandler(IValuationTriggerBuffer buffer,
        ILogger<ValuationEventBatcherHandler> logger)
    {
        _buffer = buffer;
        _logger = logger;
    }

    public Task Handle(QuoteRefreshedEvent m)
    {
        var date = DateOnly.FromDateTime(m.RetrievedUtc.ToLocalTime());
        _buffer.Add(date);
        
        _logger.LogDebug("Queued valuation date from Quote: {Date}", date.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(PriceRefreshedEvent m)
    {
        for (var d = m.StartDate; d <= m.EndDate; d = d.AddDays(1))
        {
            _buffer.Add(d);
        }
        
        _logger.LogDebug("Queued valuation dates from Price range {Start}..{End}", m.StartDate.ToIsoDateString(), m.EndDate.ToIsoDateString());
        return Task.CompletedTask;
    }

    public Task Handle(HoldingsCalculatedEvent m)
    {
        _buffer.Add(m.AsOfDate);
        
        _logger.LogDebug("Queued valuation date from Holdings: {Date}", m.AsOfDate.ToIsoDateString());
        return Task.CompletedTask;
    }
}