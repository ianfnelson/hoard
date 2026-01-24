namespace Hoard.Messages.News;

public record RefreshNewsBatchBusCommand(
    Guid NewsRunId,
    PipelineMode PipelineMode,
    int InstrumentId);
