namespace Hoard.Messages.Quotes;

public record QuoteBatchRefreshedEvent(Guid QuotesRunId, int BatchIndex);
