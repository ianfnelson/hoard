namespace Hoard.Messages.Chrono;

public record CloseOfDayCompletedEvent(
    Guid ChronoRunId,
    DateOnly AsOfDate,
    PipelineMode PipelineMode
    );