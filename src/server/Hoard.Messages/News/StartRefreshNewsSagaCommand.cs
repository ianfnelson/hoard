namespace Hoard.Messages.News;

public record StartRefreshNewsSagaCommand(
    Guid NewsRunId,
    PipelineMode PipelineMode,
    int? InstrumentId);
