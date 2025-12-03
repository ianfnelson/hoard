using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Quotes;

public class QuotesEventHandler(IMediator mediator) : 
    IHandleMessages<RefreshQuotesBatchBusCommand>,
    IHandleMessages<RefreshQuotesBusCommand>,
    IHandleMessages<QuoteChangedEvent>
{
    public async Task Handle(RefreshQuotesBusCommand message)
    {
        var applicationCommand = new ProcessRefreshQuotesCommand(message.CorrelationId, message.PipelineMode);
        
        await mediator.SendAsync(applicationCommand);
    }
    
    public async Task Handle(RefreshQuotesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshQuotesBatchCommand(message.CorrelationId, message.PipelineMode, message.InstrumentIds);
        
        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(QuoteChangedEvent message)
    {
        if (message is { PipelineMode: PipelineMode.DaytimeReactive, IsFxPair: false })
        {
            var date = DateOnly.FromDateTime(message.RetrievedUtc.ToLocalTime());
            var appCommand = new ProcessCalculateHoldingValuationsCommand(message.CorrelationId, message.PipelineMode, message.InstrumentId, date);
        
            await mediator.SendAsync(appCommand);
        }
    }
}