using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Shared;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Holdings;

public class BackfillHoldingsSaga(
    IMediator mediator,
    IBus bus,
    ILogger<BackfillHoldingsSaga> logger)
    :
        Saga<BfSagaData>,
        IAmInitiatedBy<StartBackfillHoldingsSagaCommand>,
        IHandleMessages<HoldingsCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BfSagaData> cfg)
    {
        cfg.Correlate<StartBackfillHoldingsSagaCommand>(m => m.HoldingsRunId, d => d.HoldingsRunId);
        cfg.Correlate<HoldingsCalculatedEvent>(m => m.HoldingsRunId, d => d.HoldingsRunId);
    }

    public async Task Handle(StartBackfillHoldingsSagaCommand message)
    {
        var (holdingsRunId, pipelineMode, startDate, endDate) = message;
        
        Data.HoldingsRunId = holdingsRunId;
        
        var dates = await mediator.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
            new GetDatesForBackfillQuery(startDate, endDate));
        
        logger.LogInformation("Starting holdings recomputation {Start} → {End}", dates.Min().ToIsoDateString(), dates.Max().ToIsoDateString());
        
        Data.PendingDates = dates.ToHashSet();
        
        await mediator.SendAsync(new DispatchBackfillHoldingsCommand(holdingsRunId, pipelineMode, dates));
    }
    
    public async Task Handle(HoldingsCalculatedEvent message)
    {
        Data.PendingDates.Remove(message.AsOfDate);
        if (Data.PendingDates.Count == 0)
        {
            logger.LogInformation("All holdings recomputed. Done!");
            MarkAsComplete();
            await bus.Publish(new HoldingsBackfilledEvent(
                Data.HoldingsRunId, Data.PipelineMode, Data.StartDate, Data.EndDate));
        }
    }
}

public class BfSagaData : SagaData
{
    public Guid HoldingsRunId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public HashSet<DateOnly> PendingDates { get; set; } = new();
}