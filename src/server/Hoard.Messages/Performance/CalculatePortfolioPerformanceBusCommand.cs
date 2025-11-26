namespace Hoard.Messages.Performance;

public record CalculatePortfolioPerformanceBusCommand(Guid CorrelationId, bool IsBackfill);