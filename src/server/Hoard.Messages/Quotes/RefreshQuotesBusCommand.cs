namespace Hoard.Messages.Quotes;

public record RefreshQuotesBusCommand(Guid CorrelationId, PipelineMode PipelineMode);