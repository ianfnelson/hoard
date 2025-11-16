using Hoard.Messages.Prices;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record DispatchBackfillPricesCommand(
    Guid CorrelationId,
    IReadOnlyList<int> InstrumentIds,
    DateOnly? StartDate,
    DateOnly? EndDate)
    : ICommand;

public class DispatchBackfillPricesHandler(IBus bus)
    : ICommandHandler<DispatchBackfillPricesCommand>
{
    public async Task HandleAsync(DispatchBackfillPricesCommand command, CancellationToken ct = default)
    {
        var dateRange = GetDateRange(command);
        
        var delay = TimeSpan.Zero;
        
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.DeferLocal(delay, 
                new RefreshPricesBatchBusCommand(command.CorrelationId, instrumentId, dateRange.StartDate, dateRange.EndDate, true));
            delay+=TimeSpan.FromSeconds(5);
        }   
    }
    
    private static DateRange GetDateRange(DispatchBackfillPricesCommand command)
    {
        var startDate = command.StartDate ?? DateOnlyHelper.EpochLocal();
        var endDate = command.EndDate ?? DateOnlyHelper.TodayLocal();
        
        return new DateRange(startDate, endDate);
    }
}