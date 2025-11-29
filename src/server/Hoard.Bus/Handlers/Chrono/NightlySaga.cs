using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Prices;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Chrono;
using Hoard.Messages.Holdings;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Chrono;

public class NightlySaga(IMediator mediator, ILogger<NightlySaga> logger)
: Saga<NightlySagaData>,
    IAmInitiatedBy<StartNightlySagaCommand>,
    IHandleMessages<PricesRefreshedEvent>,
    IHandleMessages<HoldingsCalculatedEvent>,
    IHandleMessages<PositionsCalculatedEvent>,
    IHandleMessages<ValuationsCalculatedEvent>,
    IHandleMessages<PerformanceCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<NightlySagaData> config)
    {
        config.Correlate<StartNightlySagaCommand>(m => m.CorrelationId, d => d.CorrelationId);

        config.Correlate<HoldingsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PositionsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PricesRefreshedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<ValuationsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartNightlySagaCommand message)
    {
        var (correlationId, nullableAsOfDate, pipelineMode) = message;
        
        var asOfDate = nullableAsOfDate.OrToday();
        
        Data.CorrelationId = correlationId;
        Data.PipelineMode = pipelineMode;
        Data.AsOfDate = asOfDate;
        
        logger.LogInformation("NightlyRun {CorrelationId}: Starting {PipelineMode} for {AsOfDate}", 
            correlationId, pipelineMode, asOfDate.ToIsoDateString());
        
        await mediator.SendAsync(new TriggerCalculateHoldingsCommand(correlationId, asOfDate, pipelineMode));

        if (pipelineMode == PipelineMode.NightlyPreMidnight)
        {
            await mediator.SendAsync(new TriggerRefreshPricesCommand(correlationId, null, asOfDate, pipelineMode));
        }
    }

    public async Task Handle(PricesRefreshedEvent message)
    {
        Data.PricesRefreshed = true;
        logger.LogInformation("NightlyRun {CorrelationId}: Prices refreshed.", Data.CorrelationId);

        if (Data.HoldingsCalculated &&
            Data.PipelineMode == PipelineMode.NightlyPreMidnight &&
            !Data.ValuationsCalculated)
        {
            logger.LogInformation("NightlyRun {CorrelationId}: Dispatching Valuations (prices now available)", Data.CorrelationId);
            await mediator.SendAsync(new TriggerCalculateHoldingsCommand(Data.CorrelationId, Data.AsOfDate, Data.PipelineMode));
        }
    }

    public async Task Handle(HoldingsCalculatedEvent message)
    {
        Data.HoldingsCalculated = true;
        logger.LogInformation("NightlyRun {CorrelationId}: Holdings calculated.", Data.CorrelationId);

        if (!Data.PositionsCalculated)
        {
            logger.LogInformation("NightlyRun {CorrelationId}: Dispatching Positions", Data.CorrelationId);
            await mediator.SendAsync(new TriggerCalculateHoldingsCommand(Data.CorrelationId, Data.AsOfDate, Data.PipelineMode));
        }
    }

    public async Task Handle(PositionsCalculatedEvent message)
    {
        Data.PositionsCalculated = true;
        logger.LogInformation("NightlyRun {CorrelationId}: Positions calculated.", Data.CorrelationId);

        var pricesSatisfied =
            Data.PipelineMode == PipelineMode.NightlyPostMidnight ||
            Data.PricesRefreshed;

        if (pricesSatisfied &&
            Data.HoldingsCalculated &&
            !Data.ValuationsCalculated)
        {
            logger.LogInformation("NightlyRun {CorrelationId}: Dispatching Valuations", Data.CorrelationId);
            await mediator.SendAsync(new TriggerCalculatePositionsCommand(Data.CorrelationId, Data.PipelineMode));
        }
    }

    public async Task Handle(ValuationsCalculatedEvent message)
    {
        Data.ValuationsCalculated = true;
        logger.LogInformation("NightlyRun {CorrelationId}: Valuations calculated.", Data.CorrelationId);
        
        logger.LogInformation("NightlyRun {CorrelationId}: Dispatching Performance", Data.CorrelationId);
        await mediator.SendAsync(new TriggerCalculatePerformanceCommand(Data.CorrelationId, null, Data.PipelineMode));
    }

    public Task Handle(PerformanceCalculatedEvent message)
    {
        logger.LogInformation("NightlyRun {CorrelationId}: Performance calculated.", Data.CorrelationId);
        MarkAsComplete();
        
        logger.LogInformation("NightlyRun {CorrelationId}: Completed {PipelineMode} for {AsOfDate}", 
            Data.CorrelationId, Data.PipelineMode, Data.AsOfDate.ToIsoDateString());
        
        return Task.CompletedTask;
    }
}

public class NightlySagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly AsOfDate { get; set; }
    public PipelineMode PipelineMode { get; set; }
    
    public bool HoldingsCalculated { get; set; }
    public bool PositionsCalculated { get; set; }
    public bool PricesRefreshed { get; set; }
    public bool ValuationsCalculated { get; set; }
}