namespace Hoard.Messages.Transactions;

public record TransactionUpdatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int Id, DateOnly Date);