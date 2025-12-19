using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Snapshots;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Chrono;
using Hoard.Messages.Holdings;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Chrono;

public class CloseOfDaySaga(IMediator mediator, ILogger<CloseOfDaySaga> logger)
: Saga<CloseOfDaySagaData>,
    IAmInitiatedBy<StartCloseOfDaySagaCommand>,
    IHandleMessages<PricesRefreshedEvent>,
    IHandleMessages<HoldingsBackfilledEvent>,
    IHandleMessages<PositionsCalculatedEvent>,
    IHandleMessages<ValuationsBackfilledEvent>,
    IHandleMessages<PerformanceCalculatedEvent>,
    IHandleMessages<SnapshotsCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CloseOfDaySagaData> config)
    {
        config.Correlate<StartCloseOfDaySagaCommand>(m => m.CorrelationId, d => d.CorrelationId);

        config.Correlate<HoldingsBackfilledEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PositionsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PricesRefreshedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<ValuationsBackfilledEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<SnapshotsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartCloseOfDaySagaCommand message)
    {
        var (correlationId, nullableAsOfDate, pipelineMode) = message;
        
        var asOfDate = nullableAsOfDate.OrToday();
        
        Data.CorrelationId = correlationId;
        Data.PipelineMode = pipelineMode;
        Data.Today = asOfDate;
        Data.Tomorrow = asOfDate.AddDays(1);
        Data.RebuildStartDate = asOfDate.AddDays(-7);
        
        logger.LogInformation("CloseOfDay {CorrelationId}: Starting {PipelineMode} for {AsOfDate}", 
            correlationId, pipelineMode, asOfDate.ToIsoDateString());
        
        logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Holdings", Data.CorrelationId);
        await mediator.SendAsync(new TriggerBackfillHoldingsCommand(correlationId, Data.RebuildStartDate, Data.Tomorrow, pipelineMode));
        
        logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Prices", Data.CorrelationId);
        await mediator.SendAsync(new TriggerRefreshPricesCommand(correlationId, null, Data.RebuildStartDate, Data.Today, pipelineMode));
    }

    public async Task Handle(PricesRefreshedEvent message)
    {
        Data.PricesRefreshed = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Prices refreshed.", Data.CorrelationId);

        if (Data.HoldingsCalculated && !Data.ValuationsCalculated)
        {
            logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Valuations (prices now available)", Data.CorrelationId);
            await mediator.SendAsync(new TriggerBackfillValuationsCommand(Data.CorrelationId,null, Data.RebuildStartDate, Data.Tomorrow, Data.PipelineMode));
        }
    }

    public async Task Handle(HoldingsBackfilledEvent message)
    {
        Data.HoldingsCalculated = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Holdings backfilled.", Data.CorrelationId);

        if (!Data.PositionsCalculated)
        {
            logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Positions", Data.CorrelationId);
            await mediator.SendAsync(new TriggerCalculatePositionsCommand(Data.CorrelationId, Data.PipelineMode));
        }
    }

    public async Task Handle(PositionsCalculatedEvent message)
    {
        Data.PositionsCalculated = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Positions calculated.", Data.CorrelationId);
        
        if (Data.PricesRefreshed &&
            Data.HoldingsCalculated &&
            !Data.ValuationsCalculated)
        {
            logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Valuations", Data.CorrelationId);
            await mediator.SendAsync(new TriggerBackfillValuationsCommand(Data.CorrelationId,null, Data.RebuildStartDate, Data.Tomorrow, Data.PipelineMode));
        }
    }

    public async Task Handle(ValuationsBackfilledEvent message)
    {
        Data.ValuationsCalculated = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Valuations calculated.", Data.CorrelationId);
        
        logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Performance", Data.CorrelationId);
        await mediator.SendAsync(new TriggerCalculatePerformanceCommand(Data.CorrelationId, null, Data.PipelineMode));
        
        logger.LogInformation("CloseOfDay {CorrelationId}: Dispatching Snapshots", Data.CorrelationId);
        await mediator.SendAsync(new TriggerCalculateSnapshotsCommand(Data.CorrelationId, Data.Today.Year, Data.PipelineMode));
    }

    public Task Handle(PerformanceCalculatedEvent message)
    {
        Data.PerformanceCalculated = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Performance calculated.", Data.CorrelationId);

        if (Data.SnapshotsCalculated)
        {
            CompleteSaga();
        }
        
        return Task.CompletedTask;
    }
    
    public Task Handle(SnapshotsCalculatedEvent message)
    {
        Data.SnapshotsCalculated = true;
        logger.LogInformation("CloseOfDay {CorrelationId}: Snapshots calculated.", Data.CorrelationId);

        if (Data.PerformanceCalculated)
        {
            CompleteSaga();
        }
        
        return Task.CompletedTask;
    }
    
    private void CompleteSaga()
    {
        MarkAsComplete();
        logger.LogInformation("CloseOfDay {CorrelationId}: Completed {PipelineMode} for {AsOfDate}", 
            Data.CorrelationId, Data.PipelineMode, Data.Today.ToIsoDateString());
    }
}

public class CloseOfDaySagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly RebuildStartDate { get; set; }
    public DateOnly Today { get; set; }
    public DateOnly Tomorrow { get; set; }
    public PipelineMode PipelineMode { get; set; }
    
    public bool HoldingsCalculated { get; set; }
    public bool PositionsCalculated { get; set; }
    public bool PricesRefreshed { get; set; }
    public bool ValuationsCalculated { get; set; }
    public bool SnapshotsCalculated { get; set; }
    public bool PerformanceCalculated { get; set; }
}