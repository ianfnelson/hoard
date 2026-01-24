namespace Hoard.Messages.News;

public record AllNewsRefreshedEvent(Guid NewsRunId, PipelineMode PipelineMode);
