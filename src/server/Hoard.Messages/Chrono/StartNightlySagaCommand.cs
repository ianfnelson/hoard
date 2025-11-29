namespace Hoard.Messages.Chrono;

public record StartNightlySagaCommand(
    Guid CorrelationId,
    DateOnly? AsOfDate,
    PipelineMode PipelineMode
    );