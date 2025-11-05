using Hoard.Core;
using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Core.Messages.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Holdings;

public class BackfillHoldingsSaga :
    Saga<BackfillHoldingsSagaData>,
    IAmInitiatedBy<BackfillHoldingsCommand>,
    IHandleMessages<HoldingsBackfilledForDateEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<BackfillHoldingsSaga> _logger;
    private readonly HoardContext _context;

    public BackfillHoldingsSaga(
        HoardContext context,
        IBus bus, 
        ILogger<BackfillHoldingsSaga> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    protected override void CorrelateMessages(ICorrelationConfig<BackfillHoldingsSagaData> cfg)
    {
        cfg.Correlate<BackfillHoldingsCommand>(m => m.BatchId, d => d.BatchId);
        cfg.Correlate<HoldingsBackfilledForDateEvent>(m => m.BatchId, d => d.BatchId);
    }

    public async Task Handle(BackfillHoldingsCommand message)
    {
        Data.BatchId = message.BatchId;
        
        var dateRange = await GetDateRange(message);
        
        _logger.LogInformation("Starting holdings recomputation {Start} → {End}", dateRange.StartDate.ToIsoDateString(), dateRange.EndDate.ToIsoDateString());
        
        Data.PendingDates = Enumerable.Range(0, dateRange.EndDate.DayNumber - dateRange.StartDate.DayNumber + 1)
            .Select(i => dateRange.StartDate.AddDays(i))
            .ToHashSet();
        
        for (var nextDate = dateRange.StartDate; nextDate <= dateRange.EndDate; nextDate = nextDate.AddDays(1))
        {
            await _bus.Send(new BackfillHoldingsForDateCommand(message.BatchId, nextDate));
        }
    }

    private async Task<DateRange> GetDateRange(BackfillHoldingsCommand message)
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

    public Task Handle(HoldingsBackfilledForDateEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            _logger.LogInformation("All holdings recomputed. Done!");
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }
}

public class BackfillHoldingsSagaData : SagaData
{
    public Guid BatchId { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}