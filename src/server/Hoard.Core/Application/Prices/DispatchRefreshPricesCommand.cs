using Hoard.Messages;
using Hoard.Messages.Prices;
using Rebus.Bus;

namespace Hoard.Core.Application.Prices;

public record DispatchRefreshPricesCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    IReadOnlyList<int> InstrumentIds,
    DateOnly? StartDate,
    DateOnly? EndDate)
    : ICommand;

public class DispatchRefreshPricesHandler(IBus bus)
    : ICommandHandler<DispatchRefreshPricesCommand>
{
    public async Task HandleAsync(DispatchRefreshPricesCommand command, CancellationToken ct = default)
    {
        var dateRange = GetDateRange(command);
        
        var delay = TimeSpan.Zero;
        
        foreach (var instrumentId in command.InstrumentIds)
        {
            await bus.DeferLocal(delay, 
                new RefreshPricesBatchBusCommand(command.CorrelationId, command.PipelineMode, instrumentId, dateRange.StartDate, dateRange.EndDate));
            delay+=TimeSpan.FromSeconds(2);
        }   
    }
    
    private static DateRange GetDateRange(DispatchRefreshPricesCommand command)
    {
        var startDate = command.StartDate ?? DateOnlyHelper.EpochLocal();
        var endDate = command.EndDate ?? DateOnlyHelper.TodayLocal();
        
        return new DateRange(startDate, endDate);
    }
}