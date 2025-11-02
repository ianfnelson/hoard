using Hoard.Core.Messages;
using Hoard.Core.Services;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers;

public class RecalculateHistoricalHoldingsCommandHandler : IHandleMessages<RecalculateHistoricalHoldingsCommand>
{
    private readonly IBus _bus;
    private readonly IHoldingsRecalculationService _holdingsRecalculationService;

    public RecalculateHistoricalHoldingsCommandHandler(IBus bus, IHoldingsRecalculationService holdingsRecalculationService)
    {
        _bus = bus;
        _holdingsRecalculationService = holdingsRecalculationService;
    }
    
    public async Task Handle(RecalculateHistoricalHoldingsCommand message)
    {
        await _holdingsRecalculationService.RecalculateHoldingsAsync(message.AsOfDate);
        await _bus.Publish(new HistoricalHoldingsRecalculatedEvent(message.BatchId, message.AsOfDate));
    }
}