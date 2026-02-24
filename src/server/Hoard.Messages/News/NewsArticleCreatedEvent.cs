namespace Hoard.Messages.News;

public record NewsArticleCreatedEvent(Guid NewsRunId, PipelineMode PipelineMode, int NewsArticleId, DateTime PublishedUtc, string Headline);