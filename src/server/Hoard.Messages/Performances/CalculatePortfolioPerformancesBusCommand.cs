namespace Hoard.Messages.Performances;

public record CalculatePortfolioPerformancesBusCommand(Guid CorrelationId, bool IsBackfill);