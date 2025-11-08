using Hoard.Core;
using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Core.Messages.Valuations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class BackfillValuationsSaga :
    Saga<BackfillValuationsSagaData>,
    IAmInitiatedBy<BackfillValuationsCommand>,
    IHandleMessages<ValuationsCalculatedEvent>
{
    private readonly HoardContext _context;
    private readonly IBus _bus;
    private readonly ILogger<BackfillValuationsSaga> _logger;

    public BackfillValuationsSaga(
        HoardContext context,
        IBus bus, 
        ILogger<BackfillValuationsSaga> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }
    
    protected override void CorrelateMessages(ICorrelationConfig<BackfillValuationsSagaData> cfg)
    {
        cfg.Correlate<BackfillValuationsCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<ValuationsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }
    
    public async Task Handle(BackfillValuationsCommand message)
    {
        Data.CorrelationId = message.CorrelationId;
        
        var dateRange = await GetDateRange(message);
        
        _logger.LogInformation("Starting valuations recomputation {Start} → {End}", dateRange.StartDate.ToIsoDateString(), dateRange.EndDate.ToIsoDateString());
        
        Data.PendingDates = Enumerable.Range(0, dateRange.EndDate.DayNumber - dateRange.StartDate.DayNumber + 1)
            .Select(i => dateRange.StartDate.AddDays(i))
            .ToHashSet();

        for (var nextDate = dateRange.StartDate; nextDate <= dateRange.EndDate; nextDate = nextDate.AddDays(1))
        {
            await _bus.SendLocal(new CalculateValuationsCommand(message.CorrelationId) { AsOfDate = nextDate });
        }
    }
    
    private async Task<DateRange> GetDateRange(BackfillValuationsCommand message)
    {
        var startDate = message.StartDate ?? await GetDefaultStartDate();
        var endDate = message.EndDate.OrToday();
        
        return new DateRange(startDate, endDate);
    }

    private async Task<DateOnly> GetDefaultStartDate()
    {
        var earliestTradeDate = await _context.Transactions
            .OrderBy(t => t.Date)
            .Select(x => x.Date)
            .FirstOrDefaultAsync();
        
        return earliestTradeDate.OrToday();
    }
    
    public Task Handle(ValuationsCalculatedEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            _logger.LogInformation("All valuations recomputed. Done!");
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }
}

public class BackfillValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}