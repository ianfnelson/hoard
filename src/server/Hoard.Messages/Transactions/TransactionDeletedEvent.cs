namespace Hoard.Messages.Transactions;

public record TransactionDeletedEvent(Guid CorrelationId, PipelineMode PipelineMode, int Id, DateOnly Date);