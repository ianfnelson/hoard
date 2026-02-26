using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Analysis;

public class AnalyseInstrumentsSaga(
    IMediator mediator,
    IBus bus,
    ILogger<AnalyseInstrumentsSaga> logger) :
    Saga<AiSagaData>,
    IAmInitiatedBy<StartAnalyseInstrumentsSagaCommand>,
    IHandleMessages<InstrumentAnalysedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<AiSagaData> cfg)
    {
        cfg.Correlate<StartAnalyseInstrumentsSagaCommand>(m => m.AnalysisRunId, d => d.AnalysisRunId);
        cfg.Correlate<InstrumentAnalysedEvent>(m => m.AnalysisRunId, d => d.AnalysisRunId);
    }

    public async Task Handle(StartAnalyseInstrumentsSagaCommand message)
    {
        var (analysisRunId, pipelineMode, asOfDate) = message;

        Data.AnalysisRunId = analysisRunId;
        Data.PipelineMode = pipelineMode;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForAnalysisQuery, IReadOnlyList<int>>(
            new GetInstrumentsForAnalysisQuery(asOfDate));

        logger.LogInformation("Started analyse instruments saga {AnalysisRunId} for {Count} instruments",
            Data.AnalysisRunId, instrumentIds.Count);

        if (instrumentIds.Count == 0)
        {
            logger.LogInformation("No instruments to analyse. Completing saga {AnalysisRunId}", Data.AnalysisRunId);
            MarkAsComplete();
            await bus.Publish(new InstrumentsAnalysedEvent(Data.AnalysisRunId, Data.PipelineMode));
            return;
        }

        Data.PendingInstruments = instrumentIds.ToHashSet();

        await mediator.SendAsync(new DispatchAnalyseInstrumentsCommand(analysisRunId, pipelineMode, instrumentIds, asOfDate));
    }

    public async Task Handle(InstrumentAnalysedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("Analyse instruments saga {AnalysisRunId} complete", Data.AnalysisRunId);
            MarkAsComplete();
            await bus.Publish(new InstrumentsAnalysedEvent(Data.AnalysisRunId, Data.PipelineMode));
        }
    }
}

public class AiSagaData : SagaData
{
    public Guid AnalysisRunId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}
