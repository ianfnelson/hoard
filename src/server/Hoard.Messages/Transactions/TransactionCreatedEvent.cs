namespace Hoard.Messages.Transactions;

public record TransactionCreatedEvent(Guid CorrelationId, int Id, DateOnly Date);