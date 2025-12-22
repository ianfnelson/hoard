using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Valuations;

public class CalculateValuationsSaga(ILogger<CalculateValuationsSaga> logger, IMediator mediator, IBus bus)
    :
        Saga<CvSagaData>,
        IAmInitiatedBy<StartCalculateValuationsSagaCommand>,
        IHandleMessages<HoldingValuationsCalculatedEvent>,
        IHandleMessages<PortfolioValuationCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CvSagaData> cfg)
    {
        cfg.Correlate<StartCalculateValuationsSagaCommand>(
            m => $"{m.ValuationsRunId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);

        cfg.Correlate<HoldingValuationsCalculatedEvent>(
            m => $"{m.ValuationsRunId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
        
        cfg.Correlate<PortfolioValuationCalculatedEvent>(
            m => $"{m.ValuationsRunId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateValuationsSagaCommand message)
    {
        var (valuationsRunId, pipelineMode, instrumentId, nullableAsOfDate) = message;
        
        Data.ValuationsRunId = valuationsRunId;

        var asOfDate = nullableAsOfDate.OrToday();
        Data.AsOfDate = asOfDate;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForHoldingValuationsQuery, IReadOnlyList<int>>(
            new GetInstrumentsForHoldingValuationsQuery(asOfDate, instrumentId));
        
        if (instrumentIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }

        logger.LogInformation("Started calculate valuations saga {CorrelationKey} for {Count} instruments",
            Data.CorrelationKey, instrumentIds.Count);

        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchCalculateHoldingValuationsCommand(valuationsRunId, pipelineMode, instrumentIds, asOfDate));
    }

    public async Task Handle(HoldingValuationsCalculatedEvent message)
    {
        var (valuationsRunId, pipelineMode, holdingId, asOfDate) = message;
        
        Data.PendingInstruments.Remove(holdingId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All holding valuations calculated");
            
            var portfolioIds = 
                await mediator.QueryAsync<GetPortfoliosForValuationQuery, IReadOnlyList<int>> (
                    new GetPortfoliosForValuationQuery());
            
            logger.LogInformation("Starting portfolio valuation calculations for {PortfolioIdsCount} portfolios", portfolioIds.Count);
            
            Data.PendingPortfolios = portfolioIds.ToHashSet();
            
            await mediator.SendAsync(new DispatchCalculatePortfolioValuationCommand(valuationsRunId, pipelineMode, portfolioIds, asOfDate));
        }
    }

    public async Task Handle(PortfolioValuationCalculatedEvent message)
    {
        var (valuationsRunId, pipelineMode, portfolioId, asOfDate) = message;
        
        Data.PendingPortfolios.Remove(portfolioId);
        if (Data.PendingPortfolios.Count == 0)
        {
            logger.LogInformation("All portfolio valuations calculated");
            
            logger.LogInformation("Calculate valuations saga {CorrelationKey} complete", Data.CorrelationKey);
            MarkAsComplete();
            
            await bus.Publish(new ValuationsCalculatedEvent(valuationsRunId, pipelineMode, asOfDate));
        }
    }
}

public class CvSagaData : SagaData
{
    public Guid ValuationsRunId { get; set; }
    public DateOnly AsOfDate { get; set; }
    
    public string CorrelationKey => $"{ValuationsRunId:N}:{AsOfDate}";
    public HashSet<int> PendingInstruments { get; set; } = new();
    public HashSet<int> PendingPortfolios { get; set; } = new();
}