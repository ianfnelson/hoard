namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePortfolioPerformancesCommand(Guid CorrelationId) : ICommand;
