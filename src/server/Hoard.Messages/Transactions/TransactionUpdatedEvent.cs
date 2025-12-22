namespace Hoard.Messages.Transactions;

public record TransactionUpdatedEvent(PipelineMode PipelineMode, int Id, DateOnly Date);