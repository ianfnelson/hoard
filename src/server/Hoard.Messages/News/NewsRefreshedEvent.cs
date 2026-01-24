namespace Hoard.Messages.News;

public record NewsRefreshedEvent(
    Guid NewsRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    DateTime RetrievedUtc);
