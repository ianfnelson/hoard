using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsBusHandler(IMediator mediator) 
    : IHandleMessages<CalculateValuationsBusCommand>,
        IHandleMessages<StockQuoteChangedEvent>,
        IHandleMessages<StockPriceChangedEvent>,
        IHandleMessages<HoldingChangedEvent>

{
    public async Task Handle(CalculateValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, message.AsOfDate, message.IsBackfill);

        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(StockQuoteChangedEvent message)
    {
        var date = DateOnly.FromDateTime(message.RetrievedUtc.ToLocalTime());
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, date);
        
        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(StockPriceChangedEvent message)
    {
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, message.AsOfDate);
        
        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(HoldingChangedEvent message)
    {
        var appCommand = new ProcessCalculateValuationsCommand(message.CorrelationId, message.InstrumentId, message.AsOfDate);
        
        await mediator.SendAsync(appCommand);
    }
}
