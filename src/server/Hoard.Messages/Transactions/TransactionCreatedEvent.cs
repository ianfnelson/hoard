namespace Hoard.Messages.Transactions;

public record TransactionCreatedEvent(PipelineMode PipelineMode, int Id, DateOnly Date);