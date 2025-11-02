using Hoard.Core.Messages;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers;

public class BackfillHistoricalHoldingsSaga :
    Saga<BackfillHistoricalHoldingsSagaData>,
    IAmInitiatedBy<BackfillHistoricalHoldingsCommand>,
    IHandleMessages<HistoricalHoldingsRecalculatedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<BackfillHistoricalHoldingsSaga> _logger;

    public BackfillHistoricalHoldingsSaga(IBus bus, ILogger<BackfillHistoricalHoldingsSaga> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override void CorrelateMessages(ICorrelationConfig<BackfillHistoricalHoldingsSagaData> cfg)
    {
        cfg.Correlate<BackfillHistoricalHoldingsCommand>(m => m.BatchId, d => d.BatchId);
        cfg.Correlate<HistoricalHoldingsRecalculatedEvent>(m => m.BatchId, d => d.BatchId);
    }

    public async Task Handle(BackfillHistoricalHoldingsCommand message)
    {
        _logger.LogInformation("Starting holdings recomputation {Start} → {End}", message.StartDate, message.EndDate);

        Data.StartDate = message.StartDate;
        Data.EndDate = message.EndDate;
        Data.PendingDates = Enumerable.Range(0, message.EndDate.DayNumber - message.StartDate.DayNumber + 1)
            .Select(i => message.StartDate.AddDays(i))
            .ToHashSet();
        
        for (var nextDate = message.StartDate; nextDate <= message.EndDate; nextDate = nextDate.AddDays(1))
        {
            await _bus.Send(new RecalculateHistoricalHoldingsCommand(message.BatchId, nextDate));
        }
    }

    public Task Handle(HistoricalHoldingsRecalculatedEvent message)
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

public class BackfillHistoricalHoldingsSagaData : SagaData
{
    public Guid BatchId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}