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
        IHandleMessages<ValuationsCalculatedForDateEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillValuationsSagaData> cfg)
    {
        cfg.Correlate<StartBackfillValuationsSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<ValuationsCalculatedForDateEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }
    
    public async Task Handle(StartBackfillValuationsSagaCommand message)
    {
        var (correlationId, startDate, endDate) = message;
        
        Data.CorrelationId = correlationId;
        
        var dates = await mediator.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
            new GetDatesForBackfillQuery(startDate, endDate));
        
        logger.LogInformation("Starting valuations recomputation {Start} → {End}", dates.Min().ToIsoDateString(), dates.Max().ToIsoDateString());

        Data.PendingDates = dates.ToHashSet();

        await mediator.SendAsync(new DispatchBackfillValuationsCommand(correlationId, dates));
    }
    
    public Task Handle(ValuationsCalculatedForDateEvent message)
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