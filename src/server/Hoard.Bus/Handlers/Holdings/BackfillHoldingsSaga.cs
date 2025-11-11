using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Extensions;
using Hoard.Messages.Holdings;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Holdings;

public class BackfillHoldingsSaga(
    IMediator mediator,
    ILogger<BackfillHoldingsSaga> logger)
    :
        Saga<BackfillHoldingsSagaData>,
        IAmInitiatedBy<StartBackfillHoldingsSagaCommand>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillHoldingsSagaData> cfg)
    {
        cfg.Correlate<StartBackfillHoldingsSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<HoldingsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartBackfillHoldingsSagaCommand message)
    {
        var (correlationId, startDate, endDate) = message;
        
        Data.CorrelationId = correlationId;
        
        var dates = await mediator.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
            new GetDatesForBackfillQuery(startDate, endDate));
        
        logger.LogInformation("Starting holdings recomputation {Start} → {End}", dates.Min().ToIsoDateString(), dates.Max().ToIsoDateString());
        
        Data.PendingDates = dates.ToHashSet();
        
        await mediator.SendAsync(new DispatchBackfillHoldingsCommand(correlationId, dates));
    }
    
    public Task Handle(HoldingsCalculatedEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            logger.LogInformation("All holdings recomputed. Done!");
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }
}

public class BackfillHoldingsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}