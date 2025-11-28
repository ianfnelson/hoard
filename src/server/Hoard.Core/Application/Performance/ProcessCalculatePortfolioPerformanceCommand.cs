using Hoard.Messages;

namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePortfolioPerformanceCommand(Guid CorrelationId, int PortfolioId, PipelineMode PipelineMode) : ICommand;
