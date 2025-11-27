namespace Hoard.Messages.Transactions;

public record TransactionCreatedEvent(Guid CorrelationId, PipelineMode PipelineMode, int Id, DateOnly Date);