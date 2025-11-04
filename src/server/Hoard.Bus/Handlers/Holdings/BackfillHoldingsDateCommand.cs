using Hoard.Core.Messages.Holdings;
using Hoard.Core.Services;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class BackfillHoldingsForDateCommandHandler : IHandleMessages<BackfillHoldingsForDateCommand>
{
    private readonly IBus _bus;
    private readonly IHoldingsCalculationService _holdingsCalculationService;

    public BackfillHoldingsForDateCommandHandler(IBus bus, IHoldingsCalculationService holdingsCalculationService)
    {
        _bus = bus;
        _holdingsCalculationService = holdingsCalculationService;
    }
    
    public async Task Handle(BackfillHoldingsForDateCommand message)
    {
        await _holdingsCalculationService.CalculateHoldingsAsync(message.AsOfDate);
        await _bus.Publish(new HoldingsBackfilledForDateEvent(message.BatchId, message.AsOfDate));
    }
}