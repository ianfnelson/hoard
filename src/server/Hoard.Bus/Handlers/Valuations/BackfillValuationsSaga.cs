using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class BackfillValuationsSaga(
    IMediator mediator,
    ILogger<BackfillValuationsSaga> logger)
    :
        Saga<BackfillValuationsSagaData>,
        IAmInitiatedBy<StartBackfillValuationsSagaCommand>,
        IHandleMessages<ValuationsCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillValuationsSagaData> cfg)
    {
        cfg.Correlate<StartBackfillValuationsSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<ValuationsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }
    
    public async Task Handle(StartBackfillValuationsSagaCommand message)
    {
        Data.CorrelationId = message.CorrelationId;
        
        var dates = await mediator.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
            new GetDatesForBackfillQuery(message.StartDate, message.EndDate));
        
        logger.LogInformation("Starting valuations recomputation {Start} → {End}", dates.Min().ToIsoDateString(), dates.Max().ToIsoDateString());

        Data.PendingDates = dates.ToHashSet();

        await mediator.SendAsync(new DispatchBackfillValuationsCommand(message.CorrelationId, dates));
    }
    
    public Task Handle(ValuationsCalculatedEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            logger.LogInformation("All valuations recomputed. Done!");
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