namespace Hoard.Messages.Transactions;

public record TransactionDeletedEvent(PipelineMode PipelineMode, int Id, DateOnly Date);