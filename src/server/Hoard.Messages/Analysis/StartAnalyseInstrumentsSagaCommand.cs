namespace Hoard.Messages.Analysis;

public record StartAnalyseInstrumentsSagaCommand(
    Guid AnalysisRunId,
    PipelineMode PipelineMode,
    DateOnly AsOfDate);
