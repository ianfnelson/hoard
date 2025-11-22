namespace Hoard.Core.Application.Performance;

public record ProcessCalculatePositionPerformancesCommand(Guid CorrelationId, int InstrumentId, bool IsBackfill = false) : ICommand;