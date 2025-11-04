using Hoard.Core.Extensions;
using Hoard.Core.Messages.Holdings;
using Hoard.Core.Services;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class CalculateHoldingsCommandHandler : IHandleMessages<CalculateHoldingsCommand>
{
    private readonly IBus _bus;
    private readonly IHoldingsCalculationService _holdingsCalculationService;

    public CalculateHoldingsCommandHandler(IBus bus, IHoldingsCalculationService holdingsCalculationService)
    {
        _bus = bus;
        _holdingsCalculationService = holdingsCalculationService;
    }
    
    public async Task Handle(CalculateHoldingsCommand message)
    {
        var asOfDate = message.AsOfDate.OrTodayIfNull();
        
        await _holdingsCalculationService.CalculateHoldingsAsync(asOfDate);
        await _bus.Publish(new HoldingsCalculatedEvent(asOfDate));
    }
}