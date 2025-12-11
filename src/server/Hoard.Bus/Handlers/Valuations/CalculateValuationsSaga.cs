using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsSaga(ILogger<CalculateValuationsSaga> logger, IMediator mediator, IBus bus)
    :
        Saga<CalculateValuationsSagaData>,
        IAmInitiatedBy<StartCalculateValuationsSagaCommand>,
        IHandleMessages<HoldingValuationsCalculatedEvent>,
        IHandleMessages<PortfolioValuationCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculateValuationsSagaData> cfg)
    {
        cfg.Correlate<StartCalculateValuationsSagaCommand>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);

        cfg.Correlate<HoldingValuationsCalculatedEvent>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
        
        cfg.Correlate<PortfolioValuationCalculatedEvent>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateValuationsSagaCommand message)
    {
        var (correlationId, pipelineMode, instrumentId, nullableAsOfDate) = message;
        
        Data.CorrelationId = correlationId;

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
        
        await mediator.SendAsync(new DispatchCalculateHoldingValuationsCommand(correlationId, pipelineMode, instrumentIds, asOfDate));
    }

    public async Task Handle(HoldingValuationsCalculatedEvent message)
    {
        var (correlationId, pipelineMode, holdingId, asOfDate) = message;
        
        Data.PendingInstruments.Remove(holdingId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All holding valuations calculated");
            
            var portfolioIds = 
                await mediator.QueryAsync<GetPortfoliosForValuationQuery, IReadOnlyList<int>> (
                    new GetPortfoliosForValuationQuery());
            
            logger.LogInformation("Starting portfolio valuation calculations for {PortfolioIdsCount} portfolios", portfolioIds.Count);
            
            Data.PendingPortfolios = portfolioIds.ToHashSet();
            
            await mediator.SendAsync(new DispatchCalculatePortfolioValuationCommand(correlationId, pipelineMode, portfolioIds, asOfDate));
        }
    }

    public async Task Handle(PortfolioValuationCalculatedEvent message)
    {
        var (correlationId, pipelineMode, portfolioId, asOfDate) = message;
        
        Data.PendingPortfolios.Remove(portfolioId);
        if (Data.PendingPortfolios.Count == 0)
        {
            logger.LogInformation("All portfolio valuations calculated");
            
            logger.LogInformation("Calculate valuations saga {CorrelationKey} complete", Data.CorrelationKey);
            MarkAsComplete();
            
            await bus.Publish(new ValuationsCalculatedEvent(correlationId, pipelineMode, asOfDate));
        }
    }
}

public class CalculateValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly AsOfDate { get; set; }
    
    public string CorrelationKey => $"{CorrelationId:N}:{AsOfDate}";
    public HashSet<int> PendingInstruments { get; set; } = new();
    public HashSet<int> PendingPortfolios { get; set; } = new();
}