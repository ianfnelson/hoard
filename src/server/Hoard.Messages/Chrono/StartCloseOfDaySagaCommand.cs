namespace Hoard.Messages.Chrono;

public record StartCloseOfDaySagaCommand(
    Guid ChronoRunId,
    DateOnly? AsOfDate,
    PipelineMode PipelineMode
    );