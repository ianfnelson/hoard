using Hoard.Core.Messages;
using Hoard.Core.Services;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers;

public class RecalculateHoldingsCommandHandler : IHandleMessages<RecalculateHoldingsCommand>
{
    private readonly IBus _bus;
    private readonly IHoldingsRecalculationService _holdingsRecalculationService;

    public RecalculateHoldingsCommandHandler(IBus bus, IHoldingsRecalculationService holdingsRecalculationService)
    {
        _bus = bus;
        _holdingsRecalculationService = holdingsRecalculationService;
    }
    
    public async Task Handle(RecalculateHoldingsCommand message)
    {
        await _holdingsRecalculationService.RecalculateHoldingsAsync(message.AsOfDate);
        await _bus.Publish(new HoldingsRecalculatedEvent(message.AsOfDate));
    }
}