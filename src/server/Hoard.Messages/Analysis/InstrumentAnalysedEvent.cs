namespace Hoard.Messages.Analysis;

public record InstrumentAnalysedEvent(
    Guid AnalysisRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    DateOnly AsOfDate,
    bool IsNewFiftyTwoWeekHigh,
    bool IsNewFiftyTwoWeekLow,
    bool IsAbnormalVolume);
