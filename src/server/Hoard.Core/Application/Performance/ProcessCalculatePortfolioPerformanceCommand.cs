namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePortfolioPerformanceCommand(Guid CorrelationId) : ICommand;
