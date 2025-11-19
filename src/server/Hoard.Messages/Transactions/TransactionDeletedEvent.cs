namespace Hoard.Messages.Transactions;

public record TransactionDeletedEvent(Guid CorrelationId, int Id, DateOnly Date);