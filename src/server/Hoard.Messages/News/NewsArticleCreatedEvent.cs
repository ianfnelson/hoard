namespace Hoard.Messages.News;

public record NewsArticleCreatedEvent(Guid NewsRunId, PipelineMode PipelineMode, int InstrumentId, int NewsArticleId, DateTime PublishedUtc, string Headline);