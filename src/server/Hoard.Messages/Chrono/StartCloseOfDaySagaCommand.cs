namespace Hoard.Messages.Chrono;

public record StartCloseOfDaySagaCommand(
    Guid CorrelationId,
    DateOnly? AsOfDate,
    PipelineMode PipelineMode
    );