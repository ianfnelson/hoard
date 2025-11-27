namespace Hoard.Messages.Quotes;

public record RefreshQuotesBatchBusCommand(Guid CorrelationId, PipelineMode PipelineMode, IReadOnlyList<int> InstrumentIds);