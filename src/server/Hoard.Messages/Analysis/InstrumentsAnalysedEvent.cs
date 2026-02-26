namespace Hoard.Messages.Analysis;

public record InstrumentsAnalysedEvent(
    Guid AnalysisRunId,
    PipelineMode PipelineMode);
