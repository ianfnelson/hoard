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
        config.Correlate<StartCloseOfDaySagaCommand>(m => m.ChronoRunId, d => d.ChronoRunId);

        config.Correlate<HoldingsBackfilledEvent>(m => m.HoldingsRunId, d => d.HoldingsRunId);
        config.Correlate<PositionsCalculatedEvent>(m => m.PositionsRunId, d => d.PositionsRunId);
        config.Correlate<PricesRefreshedEvent>(m => m.PricesRunId, d => d.PricesRunId);
        config.Correlate<ValuationsBackfilledEvent>(m => m.ValuationsRunId, d => d.ValuationsRunId);
        config.Correlate<PerformanceCalculatedEvent>(m => m.PerformanceRunId, d => d.PerformanceRunId);
        config.Correlate<SnapshotsCalculatedEvent>(m => m.SnapshotsRunId, d => d.SnapshotsRunId);
    }

    public async Task Handle(StartCloseOfDaySagaCommand message)
    {
        var (chronoRunId, nullableAsOfDate, pipelineMode) = message;
        
        var asOfDate = nullableAsOfDate.OrToday();
        
        Data.ChronoRunId = chronoRunId;
        Data.PipelineMode = pipelineMode;
        Data.Today = asOfDate;
        Data.Tomorrow = asOfDate.AddDays(1);
        Data.RebuildStartDate = asOfDate.AddDays(-7);
        
        logger.LogInformation("CloseOfDay {ChronoRunId}: Starting {PipelineMode} for {AsOfDate}", 
            chronoRunId, pipelineMode, asOfDate.ToIsoDateString());

        await DispatchPrices();
        await DispatchHoldings();
    }

    public async Task Handle(HoldingsBackfilledEvent message)
    {
        Data.HoldingsCalculated = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Holdings backfilled.", Data.ChronoRunId);

        if (!Data.PositionsCalculated)
        {
            await DispatchPositions();
        }

        if (Data.PricesRefreshed && !Data.ValuationsCalculated)
        {
            await DispatchValuations();
        }
    }

    public async Task Handle(PricesRefreshedEvent message)
    {
        Data.PricesRefreshed = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Prices refreshed.", Data.ChronoRunId);

        if (Data.HoldingsCalculated && !Data.ValuationsCalculated)
        {
            await DispatchValuations();
        }
    }

    public Task Handle(PerformanceCalculatedEvent message)
    {
        Data.PerformanceCalculated = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Performance calculated.", Data.ChronoRunId);

        if (Data.SnapshotsCalculated)
        {
            CompleteSaga();
        }
        
        return Task.CompletedTask;
    }

    public async Task Handle(PositionsCalculatedEvent message)
    {
        Data.PositionsCalculated = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Positions calculated.", Data.ChronoRunId);
        
        if (Data.ValuationsCalculated && !Data.PerformanceCalculated)
        {
            await DispatchPerformance();
        }
    }

    public async Task Handle(ValuationsBackfilledEvent message)
    {
        Data.ValuationsCalculated = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Valuations calculated.", Data.ChronoRunId);

        if (Data.PositionsCalculated && !Data.PerformanceCalculated)
        {
            await DispatchPerformance();
        }
        
        await DispatchSnapshots();
    }

    public Task Handle(SnapshotsCalculatedEvent message)
    {
        Data.SnapshotsCalculated = true;
        logger.LogInformation("CloseOfDay {ChronoRunId}: Snapshots calculated.", Data.ChronoRunId);

        if (Data.PerformanceCalculated)
        {
            CompleteSaga();
        }
        
        return Task.CompletedTask;
    }

    private async Task DispatchHoldings()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Holdings", Data.ChronoRunId);
        var triggerHoldingsCommand =
            new TriggerBackfillHoldingsCommand(Data.RebuildStartDate, Data.Tomorrow, Data.PipelineMode);
        Data.HoldingsRunId = triggerHoldingsCommand.HoldingsRunId;
        await mediator.SendAsync(triggerHoldingsCommand);
    }

    private async Task DispatchPrices()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Prices", Data.ChronoRunId);
        var triggerPricesCommand =
            new TriggerRefreshPricesCommand(null, Data.RebuildStartDate, Data.Today, Data.PipelineMode);
        Data.PricesRunId = triggerPricesCommand.PricesRunId;
        await mediator.SendAsync(triggerPricesCommand);
    }

    private async Task DispatchPerformance()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Performance", Data.ChronoRunId);
        var triggerPerformanceCommand = new TriggerCalculatePerformanceCommand(null, Data.PipelineMode);
        Data.PerformanceRunId = triggerPerformanceCommand.PerformanceRunId;
        await mediator.SendAsync(triggerPerformanceCommand);
    }

    private async Task DispatchPositions()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Positions", Data.ChronoRunId);
        var positionsCommand = new TriggerCalculatePositionsCommand(Data.PipelineMode);
        Data.PositionsRunId = positionsCommand.PositionsRunId;
        await mediator.SendAsync(positionsCommand);
    }

    private async Task DispatchSnapshots()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Snapshots", Data.ChronoRunId);
        var triggerSnapshotsCommand = new TriggerCalculateSnapshotsCommand(Data.Today.Year, Data.PipelineMode);
        Data.SnapshotsRunId = triggerSnapshotsCommand.SnapshotsRunId;
        await mediator.SendAsync(triggerSnapshotsCommand);
    }

    private async Task DispatchValuations()
    {
        logger.LogInformation("CloseOfDay {ChronoRunId}: Dispatching Valuations", Data.ChronoRunId);
        var triggerValuationsCommand =
            new TriggerBackfillValuationsCommand(null, Data.RebuildStartDate, Data.Tomorrow, Data.PipelineMode);
        Data.ValuationsRunId = triggerValuationsCommand.ValuationsRunId;
        await mediator.SendAsync(triggerValuationsCommand);
    }

    private void CompleteSaga()
    {
        MarkAsComplete();
        logger.LogInformation("CloseOfDay {ChronoRunId}: Completed {PipelineMode} for {AsOfDate}", 
            Data.ChronoRunId, Data.PipelineMode, Data.Today.ToIsoDateString());
    }
}

public class CloseOfDaySagaData : SagaData
{
    public Guid ChronoRunId { get; set; }
    public Guid PositionsRunId { get; set; }
    public Guid HoldingsRunId { get; set; }
    public Guid PricesRunId { get; set; }
    public Guid ValuationsRunId { get; set; }
    public Guid SnapshotsRunId { get; set; }
    public Guid PerformanceRunId { get; set; }
    
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