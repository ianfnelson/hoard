using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class BackfillValuationsSaga(
    IMediator mediator,
    IBus bus,
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
        var (correlationId, pipelineMode, instrumentId, startDate, endDate) = message;
        
        Data.CorrelationId = correlationId;
        Data.PipelineMode = pipelineMode;
        Data.InstrumentId = instrumentId;
        
        var dates = await mediator.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
            new GetDatesForBackfillQuery(startDate, endDate));

        Data.StartDate = dates.Min();
        Data.EndDate = dates.Max();
        
        logger.LogInformation("Starting valuations recomputation {Start} → {End}", Data.StartDate.ToIsoDateString(), Data.EndDate.ToIsoDateString());

        Data.PendingDates = dates.ToHashSet();

        await mediator.SendAsync(new DispatchBackfillValuationsCommand(correlationId, pipelineMode, instrumentId, dates));
    }
    
    public async Task Handle(ValuationsCalculatedEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            logger.LogInformation("All valuations recomputed. Done!");
            MarkAsComplete();
            await bus.Publish(new ValuationsBackfilledEvent(
                Data.CorrelationId, Data.PipelineMode, Data.InstrumentId, Data.StartDate, Data.EndDate));
        }
    }
}

public class BackfillValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public int? InstrumentId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}