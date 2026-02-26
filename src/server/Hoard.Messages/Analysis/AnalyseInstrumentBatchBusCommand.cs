namespace Hoard.Messages.Analysis;

public record AnalyseInstrumentBatchBusCommand(
    Guid AnalysisRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    DateOnly AsOfDate);
