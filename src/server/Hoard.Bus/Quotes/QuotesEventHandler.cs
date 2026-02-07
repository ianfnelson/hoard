using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Quotes;
using Rebus.Handlers;

namespace Hoard.Bus.Quotes;

public class QuotesEventHandler(IMediator mediator) : 
    IHandleMessages<RefreshQuotesBatchBusCommand>,
    IHandleMessages<RefreshQuotesBusCommand>,
    IHandleMessages<QuoteChangedEvent>
{
    public async Task Handle(RefreshQuotesBusCommand message)
    {
        var applicationCommand = new ProcessRefreshQuotesCommand();
        
        await mediator.SendAsync(applicationCommand);
    }
    
    public async Task Handle(RefreshQuotesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshQuotesBatchCommand(message.InstrumentIds);
        
        await mediator.SendAsync(appCommand);
    }
    
    public async Task Handle(QuoteChangedEvent message)
    {
        if (message.InstrumentTypeId != InstrumentType.FxPair)
        {
            var date = DateOnly.FromDateTime(message.RetrievedUtc.ToLocalTime());
            var appCommand = new ProcessCalculateHoldingValuationsCommand(Guid.NewGuid(), PipelineMode.DaytimeReactive, message.InstrumentId, date);

            await mediator.SendAsync(appCommand);
        }
    }
}